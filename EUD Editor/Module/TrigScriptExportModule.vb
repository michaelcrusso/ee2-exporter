Imports System.IO
Imports System.Text

''' <summary>
''' Collect DatEdit / Firegraft memory ops and emit TrigScript setMemory() source
''' (importable into a TrigScript / scmJS map). Button blobs become dword SetTo writes
''' into an EUD buffer; require tables become dword writes plus pointer-table patches.
''' </summary>
Module TrigScriptExportModule

    Public Const ExportSchemaVersion As Integer = 2
    ''' <summary>Bytes reserved at Vanilla for RequireData before button buffer begins.</summary>
    Public Const RequireBufferReserve As Integer = &H2000

    Public Class MemoryPatchOp
        Public Property address As Long
        Public Property addressHex As String
        Public Property modifier As String
        Public Property value As Long
        Public Property valueHex As String
        Public Property meta As Dictionary(Of String, Object)
    End Class

    Public Class PatchCollection
        Public Property ops As New List(Of MemoryPatchOp)
        Public Property requireBinary As Byte()
        Public Property buttonBufferBase As Long
        Public Property requireBufferBase As Long
        Public Property vanillaPointerAddress As Long
        Public Property fgReqUnitAddress As Long
    End Class

    Public Function CollectPatches() As PatchCollection
        Dim collection As New PatchCollection
        Dim vanillaBase As Long = ParseOffsetHex(ReadOffset("Vanilla"))
        Dim fgReqUnit As Long = ParseOffsetHex(ReadOffset("FG_ReqUnit"))
        collection.requireBufferBase = vanillaBase
        collection.buttonBufferBase = vanillaBase + RequireBufferReserve
        collection.vanillaPointerAddress = vanillaBase
        collection.fgReqUnitAddress = fgReqUnit

        If ProjectSet.UsedSetting(ProjectSet.Settingtype.DatEdit) Then
            CollectDatEditPatches(collection)
        End If

        If ProjectSet.UsedSetting(ProjectSet.Settingtype.FireGraft) Then
            ' Only emit Firegraft plumbing when something was actually edited.
            ' Enabling the Firegraft module alone used to dump the entire require.dat
            ' relocation (~1k+ dword ops) even for a single DatEdit change.
            If HasButtonModifications() Then
                CollectButtonPatches(collection)
            End If
            If HasStatusModifications() Then
                CollectStatusPatches(collection)
            End If
            If HasRequireModifications() Then
                CollectRequirePatches(collection)
            End If
        End If

        Return collection
    End Function

    Private Function HasButtonModifications() As Boolean
        If ProjectBtnUSE Is Nothing Then
            Return False
        End If
        For buttonSetIndex = 0 To ProjectBtnUSE.Count - 1
            If ProjectBtnUSE(buttonSetIndex) Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Function HasStatusModifications() As Boolean
        For unitIndex = 0 To ProjectUnitStatusFn1.Count - 1
            If ProjectUnitStatusFn1(unitIndex) <> 0 OrElse
               ProjectUnitStatusFn2(unitIndex) <> 0 OrElse
               ProjectDebugID(unitIndex) <> 0 Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Function HasRequireModifications() As Boolean
        If ProjectRequireDataUSE Is Nothing Then
            Return False
        End If
        For tableIndex = 0 To ProjectRequireDataUSE.Count - 1
            If ProjectRequireDataUSE(tableIndex) Is Nothing Then
                Continue For
            End If
            For entryIndex = 0 To ProjectRequireDataUSE(tableIndex).Count - 1
                If ProjectRequireDataUSE(tableIndex)(entryIndex) <> 0 Then
                    Return True
                End If
            Next
        Next
        Return False
    End Function

    ''' <summary>
    ''' Write patch source for the selected export format into outputDirectory.
    ''' </summary>
    Public Function ExportToDirectory(outputDirectory As String) As String
        If String.IsNullOrWhiteSpace(outputDirectory) Then
            Throw New ArgumentException("Output directory is required.")
        End If
        If Not Directory.Exists(outputDirectory) Then
            Directory.CreateDirectory(outputDirectory)
        End If

        Dim collection As PatchCollection = CollectPatches()
        Select Case ProjectSet.ExportFormat
            Case ProjectSet.PatchExportFormat.TrigScript
                Dim trigPath As String = Path.Combine(outputDirectory, "eud-patches.trig")
                File.WriteAllText(trigPath, BuildTrigScriptSource(collection), New UTF8Encoding(False))
                Return trigPath
            Case ProjectSet.PatchExportFormat.EpScript
                Dim epsPath As String = Path.Combine(outputDirectory, "eud-patches.eps")
                File.WriteAllText(epsPath, BuildEpScriptSource(collection), New UTF8Encoding(False))
                Return epsPath
            Case ProjectSet.PatchExportFormat.ClassicTrigger
                Throw New NotImplementedException("Classic TrigEdit export is not implemented yet (needs EPD/player-unit encoding).")
            Case Else
                Throw New InvalidOperationException("Unknown export format.")
        End Select
    End Function

    ''' <summary>TrigScript source: one program that applies setMemory ops at start.</summary>
    Public Function BuildTrigScriptSource(collection As PatchCollection) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("// Generated by EE2 Exporter — TrigScript memory patches.")
        AppendCommonHeaderComments(sb)
        sb.AppendLine("// Paste/import into a TrigScript map. Ops run once at program start.")
        sb.AppendLine()
        sb.AppendLine("program(() => {")
        AppendMemoryOps(sb, collection, "  ", "setMemory")
        sb.AppendLine("});")
        sb.AppendLine()
        Return sb.ToString()
    End Function

    ''' <summary>epScript source: onPluginStart with SetMemory actions (euddraft/epScript).</summary>
    Public Function BuildEpScriptSource(collection As PatchCollection) As String
        Dim sb As New StringBuilder()
        sb.AppendLine("// Generated by EE2 Exporter — epScript memory patches.")
        AppendCommonHeaderComments(sb)
        sb.AppendLine("// Drop into an euddraft .eps plugin / include. Runs once at plugin start.")
        sb.AppendLine()
        sb.AppendLine("function onPluginStart() {")
        AppendMemoryOps(sb, collection, "    ", "SetMemory")
        sb.AppendLine("}")
        sb.AppendLine()
        Return sb.ToString()
    End Function

    Private Sub AppendCommonHeaderComments(sb As StringBuilder)
        sb.AppendLine("// schemaVersion " & ExportSchemaVersion & "; starVersion " & ProgramSet.StarVersion)
        If Not String.IsNullOrEmpty(ProjectSet.filename) Then
            sb.AppendLine("// project: " & ProjectSet.filename.Replace("\", "/"))
        End If
    End Sub

    Private Sub AppendMemoryOps(sb As StringBuilder, collection As PatchCollection, indent As String, functionName As String)
        If collection.ops.Count = 0 Then
            sb.AppendLine(indent & "// (no memory ops — DatEdit/Firegraft deltas are empty)")
            Return
        End If
        For Each op In collection.ops
            Dim comment As String = FormatOpComment(op)
            If comment <> "" Then
                sb.AppendLine(indent & "// " & comment)
            End If
            sb.AppendLine(indent & functionName & "(" & op.addressHex & ", " & op.modifier & ", " & op.valueHex & ");")
        Next
    End Sub

    Private Function FormatOpComment(op As MemoryPatchOp) As String
        If op.meta Is Nothing OrElse op.meta.Count = 0 Then
            Return ""
        End If
        Dim source As Object = Nothing
        If op.meta.TryGetValue("source", source) AndAlso source IsNot Nothing Then
            Select Case source.ToString()
                Case "datEdit"
                    Return String.Format("{0} {1} objectId={2} delta={3}",
                                        op.meta("table"), op.meta("field"), op.meta("objectId"), op.meta("delta"))
                Case "buttonPayload"
                    Return String.Format("buttonPayload set={0} off={1}", op.meta("buttonSetId"), op.meta("payloadOffset"))
                Case "buttonPointer", "buttonCount"
                    Return source.ToString() & " set=" & op.meta("buttonSetId").ToString()
                Case "requirePayload"
                    Return "requirePayload off=" & op.meta("payloadOffset").ToString()
                Case Else
                    Return source.ToString()
            End Select
        End If
        Return ""
    End Function

    Public Function TryExportWithFolderDialog(owner As IWin32Window) As Boolean
        If ProjectSet.isload = False Then
            MsgBox("Open a project before exporting.", MsgBoxStyle.Exclamation, ProgramSet.ErrorFormMessage)
            Return False
        End If

        Using dialog As New FolderBrowserDialog()
            dialog.Description = "Export memory patches to folder"
            dialog.ShowNewFolderButton = True
            Dim startDirectory As String = ResolveExportDialogDirectory()
            If startDirectory <> "" Then
                dialog.SelectedPath = startDirectory
            End If
            If dialog.ShowDialog(owner) <> DialogResult.OK Then
                Return False
            End If
            Try
                Dim pathWritten As String = ExportToDirectory(dialog.SelectedPath)
                RememberExportDirectory(dialog.SelectedPath)
                MsgBox("Exported " & pathWritten, MsgBoxStyle.Information, "Export")
                Return True
            Catch ex As Exception
                MsgBox("Export failed: " & ex.Message, MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
                Return False
            End Try
        End Using
    End Function

    Private Sub CollectDatEditPatches(collection As PatchCollection)
        For tableIndex = 0 To DatEditDATA.Count - 1
            Dim table = DatEditDATA(tableIndex)
            For fieldIndex = 0 To table.projectdata.Count - 1
                Dim fieldName As String = table.keyDic.Keys.ToList(fieldIndex)
                Dim fieldSize As Integer = table.keyINFO(fieldIndex).realSize
                For objectIndex = 0 To table.projectdata(fieldIndex).Count - 1
                    Dim delta As Long = table.projectdata(fieldIndex)(objectIndex)
                    If delta = 0 Then Continue For

                    Dim offsetName As String = table.typeName & "_" & fieldName
                    Dim absoluteAddress As Long = ParseOffsetHex(ReadOffset(offsetName)) + fieldSize * objectIndex
                    Dim byteLane As Long = absoluteAddress Mod 4
                    Dim dwordAddress As Long = absoluteAddress - byteLane
                    Dim packedValue As Long = delta * CLng(Math.Pow(256, byteLane))
                    Dim modifier As String
                    Dim emitValue As Long
                    If packedValue >= 0 Then
                        modifier = "Add"
                        emitValue = packedValue
                    Else
                        modifier = "Subtract"
                        emitValue = -packedValue
                    End If

                    Dim objectId As Integer = objectIndex + table.keyINFO(fieldIndex).VarStart
                    AddOp(collection, dwordAddress, modifier, emitValue, New Dictionary(Of String, Object) From {
                        {"source", "datEdit"},
                        {"table", table.typeName},
                        {"tableIndex", tableIndex},
                        {"field", fieldName},
                        {"fieldIndex", fieldIndex},
                        {"objectId", objectId},
                        {"size", fieldSize},
                        {"delta", delta},
                        {"byteLane", byteLane},
                        {"effectiveAddress", FormatHex(absoluteAddress)}
                    })
                Next
            Next
        Next
    End Sub

    Private Sub CollectButtonPatches(collection As PatchCollection)
        Dim btnAddressBase As Long = ParseOffsetHex(ReadOffset("FG_BtnAddress"))
        Dim btnNumBase As Long = ParseOffsetHex(ReadOffset("FG_BtnNum"))
        Dim bufferCursor As Long = collection.buttonBufferBase

        For buttonSetIndex = 0 To ProjectBtnUSE.Count - 1
            If ProjectBtnUSE(buttonSetIndex) = False Then Continue For

            Dim payload As Byte() = SerializeButtonSetBytes(buttonSetIndex)
            Dim pointerSlot As Long = btnAddressBase + 12 * buttonSetIndex
            Dim countSlot As Long = btnNumBase + 12 * buttonSetIndex
            Dim payloadAddress As Long = AlignDword(bufferCursor)

            ' Expand blob into dword SetTo writes (TrigScript-friendly; no Db()).
            Dim offsetInBlob As Integer = 0
            While offsetInBlob < payload.Length
                Dim dwordValue As Long = 0
                For byteIndex = 0 To 3
                    If offsetInBlob + byteIndex < payload.Length Then
                        dwordValue += CLng(payload(offsetInBlob + byteIndex)) * CLng(Math.Pow(256, byteIndex))
                    End If
                Next
                AddOp(collection, payloadAddress + offsetInBlob, "SetTo", dwordValue, New Dictionary(Of String, Object) From {
                    {"source", "buttonPayload"},
                    {"buttonSetId", buttonSetIndex},
                    {"payloadOffset", offsetInBlob},
                    {"payloadLength", payload.Length}
                })
                offsetInBlob += 4
            End While

            AddOp(collection, pointerSlot, "SetTo", payloadAddress, New Dictionary(Of String, Object) From {
                {"source", "buttonPointer"},
                {"buttonSetId", buttonSetIndex}
            })
            AddOp(collection, countSlot, "SetTo", ProjectBtnData(buttonSetIndex).Count, New Dictionary(Of String, Object) From {
                {"source", "buttonCount"},
                {"buttonSetId", buttonSetIndex}
            })

            bufferCursor = payloadAddress + AlignDword(Math.Max(payload.Length, 4))
        Next
    End Sub

    Private Sub CollectStatusPatches(collection As PatchCollection)
        Dim offsetKeys() As String = {"FG_Debug", "FG_Status", "FG_Display"}

        For unitIndex = 0 To ProjectUnitStatusFn1.Count - 1
            For statusKind = 0 To 2
                Dim checkValue As Long
                Dim deltaValue As Long
                Dim fieldLabel As String

                Select Case statusKind
                    Case 0
                        checkValue = ProjectDebugID(unitIndex)
                        deltaValue = ProjectDebugID(unitIndex)
                        fieldLabel = "debugId"
                    Case 1
                        checkValue = ProjectUnitStatusFn1(unitIndex)
                        deltaValue = CLng(statusFn1(ProjectUnitStatusFn1(unitIndex) + UnitStatusFn1(unitIndex))) - statusFn1(UnitStatusFn1(unitIndex))
                        fieldLabel = "statusFn"
                    Case Else
                        checkValue = ProjectUnitStatusFn2(unitIndex)
                        deltaValue = CLng(statusFn2(ProjectUnitStatusFn2(unitIndex) + UnitStatusFn2(unitIndex))) - statusFn2(UnitStatusFn2(unitIndex))
                        fieldLabel = "displayFn"
                End Select

                If checkValue = 0 Then Continue For

                Dim address As Long = ParseOffsetHex(ReadOffset(offsetKeys(statusKind))) + 12 * unitIndex
                Dim modifier As String
                Dim emitValue As Long
                If deltaValue >= 0 Then
                    modifier = "Add"
                    emitValue = deltaValue
                Else
                    modifier = "Subtract"
                    emitValue = -deltaValue
                End If

                AddOp(collection, address, modifier, emitValue, New Dictionary(Of String, Object) From {
                    {"source", "firegraftStatus"},
                    {"field", fieldLabel},
                    {"unitId", unitIndex},
                    {"offsetKey", offsetKeys(statusKind)}
                })
            Next
        Next
    End Sub

    Private Sub CollectRequirePatches(collection As PatchCollection)
        collection.requireBinary = BuildRequireDataBytes()

        ' Dword-write the require opcode blob at Vanilla (same destination as legacy dataDumper).
        Dim requireBase As Long = collection.requireBufferBase
        Dim offsetInBlob As Integer = 0
        While offsetInBlob < collection.requireBinary.Length
            Dim dwordValue As Long = 0
            For byteIndex = 0 To 3
                If offsetInBlob + byteIndex < collection.requireBinary.Length Then
                    dwordValue += CLng(collection.requireBinary(offsetInBlob + byteIndex)) * CLng(Math.Pow(256, byteIndex))
                End If
            Next
            AddOp(collection, requireBase + offsetInBlob, "SetTo", dwordValue, New Dictionary(Of String, Object) From {
                {"source", "requirePayload"},
                {"payloadOffset", offsetInBlob},
                {"payloadLength", collection.requireBinary.Length}
            })
            offsetInBlob += 4
        End While

        ' Pointer tables into the require blob (same math as RepDataToTrigger).
        Dim pointerKeys() As String = {"FG_PReqUnit", "FG_PReqUpg", "FG_PReqTechUpg", "FG_PReqTechUse", "FG_PReqOrder"}
        For tableIndex = 0 To 4
            Dim pointerBase As Long = ParseOffsetHex(ReadOffset(pointerKeys(tableIndex)))
            Dim packed As UInteger = 0
            For entryIndex = 0 To RequireData(tableIndex).Count - 1
                Dim posValue As UInteger
                If ProjectRequireDataUSE(tableIndex)(entryIndex) = 0 Then
                    posValue = RequireData(tableIndex)(entryIndex).pos
                Else
                    posValue = ProjectRequireData(tableIndex)(entryIndex).pos
                End If

                If entryIndex Mod 2 = 0 Then
                    packed = posValue
                Else
                    packed += posValue * 65536UI
                    Dim address As Long = pointerBase + (entryIndex * 2) - 2
                    AddOp(collection, address, "SetTo", packed, New Dictionary(Of String, Object) From {
                        {"source", "requirePointerTable"},
                        {"table", pointerKeys(tableIndex)},
                        {"tableIndex", tableIndex},
                        {"entryPair", entryIndex \ 2}
                    })
                End If
            Next
        Next

        ' Legacy redirect: Vanilla cell → FG_ReqUnit (kept for parity with GetPYtext).
        AddOp(collection, collection.vanillaPointerAddress, "SetTo", collection.fgReqUnitAddress, New Dictionary(Of String, Object) From {
            {"source", "requireBaseRedirect"},
            {"note", "Matches legacy SetMemory(Vanilla, SetTo, FG_ReqUnit). Require blob dwords are also written at Vanilla."}
        })
    End Sub

    ''' <summary>Button set bytes sorted by pos, little-endian field layout matching GetTextValue.</summary>
    Public Function SerializeButtonSetBytes(buttonSetIndex As Integer) As Byte()
        If ProjectBtnData(buttonSetIndex) Is Nothing OrElse ProjectBtnData(buttonSetIndex).Count = 0 Then
            Return New Byte() {}
        End If

        Dim order As New List(Of Integer)
        For i = 0 To ProjectBtnData(buttonSetIndex).Count - 1
            order.Add(i)
        Next
        order.Sort(Function(a, b) ProjectBtnData(buttonSetIndex)(a).pos.CompareTo(ProjectBtnData(buttonSetIndex)(b).pos))

        Using mem As New MemoryStream()
            Using writer As New BinaryWriter(mem, Encoding.UTF8, leaveOpen:=True)
                For Each sortedIndex In order
                    Dim button = ProjectBtnData(buttonSetIndex)(sortedIndex)
                    writer.Write(button.pos)
                    writer.Write(button.icon)
                    writer.Write(button.con)
                    writer.Write(button.act)
                    writer.Write(button.conval)
                    writer.Write(button.actval)
                    writer.Write(button.enaStr)
                    writer.Write(button.disStr)
                Next
                writer.Flush()
            End Using
            Return mem.ToArray()
        End Using
    End Function

    Private Sub AddOp(collection As PatchCollection, address As Long, modifier As String, value As Long, meta As Dictionary(Of String, Object))
        Dim op As New MemoryPatchOp With {
            .address = address,
            .addressHex = FormatHex(address),
            .modifier = modifier,
            .value = value,
            .valueHex = FormatHex(value),
            .meta = meta
        }
        collection.ops.Add(op)
    End Sub

    Private Function ParseOffsetHex(offsetText As String) As Long
        Dim cleaned As String = offsetText.Trim()
        If cleaned.StartsWith("0x", StringComparison.OrdinalIgnoreCase) Then
            cleaned = cleaned.Substring(2)
        End If
        Return Convert.ToInt64(cleaned, 16)
    End Function

    Private Function FormatHex(value As Long) As String
        If value < 0 Then
            Return "-0x" & Hex(-value)
        End If
        Return "0x" & Hex(value)
    End Function

    Private Function AlignDword(value As Long) As Long
        Dim remainder = value Mod 4
        If remainder = 0 Then Return value
        Return value + (4 - remainder)
    End Function

End Module

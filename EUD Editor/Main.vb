'맵에 삽입은 공통 사용.
'euddraft사용 체크하면 py넣고 아니면 맵에 직접 넣는 거지.

'Size1 423, 212
'Size2 489, 212
'Size3 423, 159


Imports System.IO

Public Class Main

    Dim RecentlyOpenedFiles As New ArrayList()

    Public Sub refreshSet()
        ThemeSetForm.SetControlColor(Me)

        ' TileSet / Plugin remain unsupported regardless of saved project flags.
        ProjectSet.UsedSetting(ProjectSet.Settingtype.TileSet) = False
        ProjectSet.UsedSetting(ProjectSet.Settingtype.Plugin) = False

        If ProjectSet.isload = True Then
            If ProgramSet.StarVersion = "Remastered" Then
                CheckCompatiblity()
            End If
        End If


        buttonResetting()
        menuResetting()
        nameResetting()
    End Sub

    Public Sub buttonResetting()
        If ProgramSet.StarVersion = "1.16.1" Then
            Me.MinimumSize = New Size(423 + 66, 212)
            Me.Size = New Size(423 + 66, 212)
        Else
            Me.MinimumSize = New Size(423 + 66 + 65 + 66, 159)
            Me.Size = New Size(423 + 66 + 65 + 66, 159)
        End If

        FlowLayoutPanel2.Visible = False
        Button6.Enabled = False
        Button6.Visible = False
        ' Plugin panel is euddraft-era; hide for this TrigScript-oriented fork.
        ProjectSet.UsedSetting(ProjectSet.Settingtype.Plugin) = False
        Button10.Enabled = False
        Button10.Visible = False

        Dim loaded As Boolean = ProjectSet.isload
        Button2.Enabled = loaded AndAlso ProjectSet.UsedSetting(0)
        Button3.Enabled = loaded AndAlso ProjectSet.UsedSetting(1)
        CheckBox1.Enabled = loaded
        CheckBox1.Checked = ProgramSet.isAutoCompile
        CheckBox1.Text = "AutoExport"
        Button14.Enabled = loaded
        Button14.Text = "&Export"
        ExportTrigScriptToolStripMenuItem.Enabled = loaded
        SyncExportFormatMenu()

        FlowLayoutPanel4.AutoSize = True
        If ProgramSet.StarVersion = "1.16.1" Then
            Button5.Enabled = loaded AndAlso ProjectSet.UsedSetting(2)
            Button5.Visible = True
            Button7.Enabled = loaded AndAlso ProjectSet.UsedSetting(4)
            Button8.Enabled = loaded AndAlso ProjectSet.UsedSetting(5)
            Button8.Visible = True
            Button9.Enabled = loaded AndAlso ProjectSet.UsedSetting(6)
            Button10.Enabled = False
            Button10.Visible = False
            Button15.Enabled = loaded AndAlso ProjectSet.UsedSetting(8)
            Button11.Visible = True
            Button11.Enabled = loaded
            Button12.Visible = True
            Button12.Enabled = loaded
            Button13.Visible = True
            Button13.Enabled = loaded
        Else
            Button5.Enabled = False
            Button5.Visible = False
            Button7.Enabled = loaded AndAlso ProjectSet.UsedSetting(4)
            Button8.Enabled = False
            Button8.Visible = False
            Button9.Enabled = loaded AndAlso ProjectSet.UsedSetting(6)
            Button10.Enabled = False
            Button10.Visible = False
            Button15.Enabled = loaded AndAlso ProjectSet.UsedSetting(8)
            Button11.Visible = True
            Button11.Enabled = loaded
            Button12.Visible = False
            Button12.Enabled = False
            Button13.Visible = False
            Button13.Enabled = False
        End If
    End Sub

    Public Sub menuResetting()
        SaveToolStripMenuItem.Enabled = ProjectSet.isload
        SaveasToolStripMenuItem.Enabled = ProjectSet.isload
        ProCloseToolStripMenuItem.Enabled = ProjectSet.isload
        ExportTrigScriptToolStripMenuItem.Enabled = ProjectSet.isload
        btn_Save.Enabled = ProjectSet.isload
        btn_close.Enabled = ProjectSet.isload
        SyncExportFormatMenu()
    End Sub

    Private Sub SyncExportFormatMenu()
        ExportFormatTrigScriptMenuItem.Checked = (ProjectSet.ExportFormat = ProjectSet.PatchExportFormat.TrigScript)
        ExportFormatEpScriptMenuItem.Checked = (ProjectSet.ExportFormat = ProjectSet.PatchExportFormat.EpScript)
        ExportFormatClassicMenuItem.Checked = (ProjectSet.ExportFormat = ProjectSet.PatchExportFormat.ClassicTrigger)
        ExportFormatTrigScriptMenuItem.Enabled = True
        ExportFormatEpScriptMenuItem.Enabled = True
        ' Classic TrigEdit needs EPD/player-unit encoding — leave stubbed for now.
        ExportFormatClassicMenuItem.Enabled = False
    End Sub

    Private Sub ExportFormatTrigScriptMenuItem_Click(sender As Object, e As EventArgs) Handles ExportFormatTrigScriptMenuItem.Click
        ProjectSet.ExportFormat = ProjectSet.PatchExportFormat.TrigScript
        ProjectSet.saveStatus = False
        SyncExportFormatMenu()
    End Sub

    Private Sub ExportFormatEpScriptMenuItem_Click(sender As Object, e As EventArgs) Handles ExportFormatEpScriptMenuItem.Click
        ProjectSet.ExportFormat = ProjectSet.PatchExportFormat.EpScript
        ProjectSet.saveStatus = False
        SyncExportFormatMenu()
    End Sub

    Private Sub ExportFormatClassicMenuItem_Click(sender As Object, e As EventArgs) Handles ExportFormatClassicMenuItem.Click
        ' Stub until classic TrigEdit EPD encoding is implemented.
        SyncExportFormatMenu()
    End Sub




    Public Sub nameResetting()
        Dim issaved As String

        If ProjectSet.saveStatus Then
            issaved = "  "
        Else
            issaved = " *"
        End If

        If ProjectSet.isload = True Then
            If ProjectSet.filename = "" Then
                Me.Text = Lan.GetMsgText("Defacultname") & " " & issaved & " -  EE2 Exporter " & ProgramSet.Version & "." & ProgramSet.StarVersion

                If DatEditForm IsNot Nothing Then DatEditForm.Text = ProgramSet.DatEditName & issaved & " " & ProgramSet.Version

                If FireGraftForm IsNot Nothing Then FireGraftForm.Text = ProgramSet.FireGraftName & issaved & " " & ProgramSet.Version
            Else
                Dim name As String = ProjectSet.filename.Split("\").Last

                Me.Text = name & issaved & " -  EE2 Exporter " & ProgramSet.Version & "." & ProgramSet.StarVersion
                If DatEditForm IsNot Nothing Then DatEditForm.Text = name & issaved & " - " & ProgramSet.DatEditName & " " & ProgramSet.Version

                If FireGraftForm IsNot Nothing Then FireGraftForm.Text = name & issaved & " - " & ProgramSet.FireGraftName & " " & ProgramSet.Version
            End If
        Else
            Me.Text = "EE2 Exporter " & ProgramSet.Version & "." & ProgramSet.StarVersion
        End If

    End Sub



    Private Sub Main_SizeChanged(sender As Object, e As EventArgs) Handles MyBase.SizeChanged
        If ProgramSet.StarVersion = "1.16.1" Then
            If Size.Height > (313 - 52) Then '313
                Button12.Size = New Size(65, 50)
                TableLayoutPanel2.ColumnStyles.Item(0).Width = 67
            Else
                Button12.Size = New Size(65, 102)
                TableLayoutPanel2.ColumnStyles.Item(0).Width = 135
            End If
        Else
            If Size.Height > (313 - 52 - 52) Then '313
                Button12.Size = New Size(65, 50)
                TableLayoutPanel2.ColumnStyles.Item(0).Width = 67
            Else
                Button12.Size = New Size(65, 102)
                TableLayoutPanel2.ColumnStyles.Item(0).Width = 135
            End If
        End If
    End Sub

    Dim ShutDown As Boolean = False
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load



        'My.Settings.Reset()
        If init() = False Then
            ShutDown = True
            Me.Close()
        End If
        Lan.SetMenu(Me, MenuStrip1)
        Lan.SetLanguage(Me)

        SaveFileDialog1.Filter = Lan.GetText(Me.Name, "SaveFilter")
        OpenFileDialog1.Filter = Lan.GetText(Me.Name, "OpenFilter")

        RefreshRecentlyOpenedList()

        refreshSet()
    End Sub

    Private Sub Main_Closed(sender As Object, e As FormClosingEventArgs) Handles MyBase.Closing
        If ShutDown = False Then
            'Dim filename As String = My.Application.Info.DirectoryPath & "\Data\temp"
            'System.IO.Directory.Delete(path, False)

            'Try
            '    Dim path As String = My.Application.Info.DirectoryPath & "\Data\temp"
            '    Dim di As New IO.DirectoryInfo(path)
            '    di.Delete(True)
            'Catch ex As Exception

            'End Try



            If ProjectSet.Close() Then
                'StarCraftVisibleForm.Close()

                My.Settings.StarDirec = ProgramSet.StarDirec
                My.Settings.euddraftDirec = ProgramSet.euddraftDirec
                My.Settings.StarVersion = ProgramSet.StarVersion
                My.Settings.AutoCompile = ProgramSet.isAutoCompile

                SaveTheme()

                My.Settings.mpqDirec = String.Join(",", ProgramSet.DatMPQDirec)

                My.Settings.Save()
            Else
                e.Cancel = True
            End If
        End If
    End Sub

    Sub SaveTheme()
        My.Settings.DatEditColor1 = ProgramSet.colorFieldText
        My.Settings.DatEditColor2 = ProgramSet.colorFieldBackground
        My.Settings.DatEditColor3 = ProgramSet.colorChangedBackground
        My.Settings.DatEditColor4 = ProgramSet.colorCheckedBackground
        My.Settings.DatEditColor5 = ProgramSet.colorBackground
        My.Settings.DatEditColor6 = ProgramSet.colorLabelText
        My.Settings.DatEditColor7 = ProgramSet.colorCodeBackground
        My.Settings.DatEditColor8 = ProgramSet.colorPanelBackground
        My.Settings.Save()
    End Sub


    Private Sub Setting_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SettingForm.PreSizeSet()
        SettingForm.ShowDialog()


        ProjectSet.LoadCHKdata()
        DatEditForm.ReloadCHK()
        refreshSet()
        CheckBox1.Checked = ProgramSet.isAutoCompile
        'My.Forms.SettingForm.Location = Me.Location + Button1.Location + New Point(0, 105)
    End Sub

    Private Sub 새로만들기NToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NewNToolStripMenuItem.Click
        새로만들기()
    End Sub
    Private Sub 새로만들기()
        If ProjectSet.Close() = True Then
            ProjectSet.loading = True

            ProjectSet.Reset()
            SettingForm.PreSizeSet()
            SettingForm.ShowDialog()


            If ProjectSet.loading = True Then
                ProjectSet.isload = True
                ProjectSet.saveStatus = True
            Else '취소 할 경우
                ProjectSet.Close()
            End If

            ProjectSet.LoadCHKdata()
            LoadFileimportable()
            ProjectSet.loading = False
            refreshSet()
        End If
    End Sub

    Private Sub 프로젝트닫기ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ProCloseToolStripMenuItem.Click
        프로젝트닫기()
    End Sub
    Private Sub 프로젝트닫기()
        ProjectSet.Close()

        refreshSet()
    End Sub

    Private Sub 다른이름으로저장ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveasToolStripMenuItem.Click
        다른이름으로저장()
    End Sub
    Private Sub 다른이름으로저장()
        Dim Dialog As DialogResult
        Dim projectDir As String = ResolveProjectDialogDirectory()
        Dim baseName As String
        If ProjectSet.filename = "" Then
            baseName = Lan.GetMsgText("Defacultname")
        Else
            baseName = Path.GetFileNameWithoutExtension(ProjectSet.filename)
        End If
        If projectDir <> "" Then
            SaveFileDialog1.InitialDirectory = projectDir
            ' Full path helps Wine return a rooted FileName after OK.
            SaveFileDialog1.FileName = Path.Combine(projectDir, baseName)
        Else
            SaveFileDialog1.FileName = baseName
        End If

        Dialog = ShowFileDialog(SaveFileDialog1)

        If Dialog = DialogResult.Cancel Then
        Else
            ProjectSet.Save(SaveFileDialog1.FileName)
            RememberProjectDirectory(SaveFileDialog1.FileName)
        End If

        refreshSet()
    End Sub

    Private Sub 열기ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        열기()
    End Sub
    Private Sub 열기()
        Dim Dialog As DialogResult

        Dim projectDir As String = ResolveProjectDialogDirectory()
        If projectDir <> "" Then
            OpenFileDialog1.InitialDirectory = projectDir
        End If

        Dialog = ShowFileDialog(OpenFileDialog1)
        If Dialog = DialogResult.Cancel Then
        Else
            If ProjectSet.Close() = True Then
                ProjectSet.Load(OpenFileDialog1.FileName)
                RememberProjectDirectory(OpenFileDialog1.FileName)
                If ProjectSet.ExportDirectory <> "" Then
                    RememberExportDirectory(ProjectSet.ExportDirectory)
                End If
                CheckMapFile()
                ' Map-less TrigScript projects have an empty InputMap — skip watch stamp.
                If ProjectSet.InputMap <> "" AndAlso CheckFileExist(ProjectSet.InputMap) = False Then
                    Dim fileinfo As New FileInfo(ProjectSet.InputMap)
                    LastData = fileinfo.LastWriteTime
                End If
            End If
        End If


        refreshSet()
    End Sub

    Private Sub 저장ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveToolStripMenuItem.Click
        저장()
    End Sub
    Public Sub 저장()
        Dim extension As String = ProjectSet.filename.Split(".").Last
        'Dim ise2s As Boolean = False
        'Try
        '    If Mid(ProjectSet.filename, ProjectSet.filename.Length - 3) <> ".e2s" Then
        '        ise2s = True
        '    End If
        'Catch ex As Exception

        'End Try


        If ProjectSet.filename = "" Or extension = "ees" Or extension = "mem" Then 'Or ise2s Then
            Dim Dialog As DialogResult
            Dim projectDir As String = ResolveProjectDialogDirectory()
            Dim baseName As String
            If ProjectSet.filename = "" Then
                baseName = Lan.GetMsgText("Defacultname")
            Else
                baseName = Path.GetFileNameWithoutExtension(ProjectSet.filename)
            End If
            If projectDir <> "" Then
                SaveFileDialog1.InitialDirectory = projectDir
                SaveFileDialog1.FileName = Path.Combine(projectDir, baseName)
            Else
                SaveFileDialog1.FileName = baseName
            End If

            Dialog = ShowFileDialog(SaveFileDialog1)
            If Dialog = DialogResult.Cancel Then
            Else
                ProjectSet.Save(SaveFileDialog1.FileName)
                RememberProjectDirectory(SaveFileDialog1.FileName)
            End If
        Else
            ProjectSet.Save(ProjectSet.filename)
            RememberProjectDirectory(ProjectSet.filename)
        End If
        ' AutoCompile checkbox: when on, re-export to the last TrigScript folder (no euddraft).
        If ProgramSet.isAutoCompile = True AndAlso ProjectSet.ExportDirectory <> "" AndAlso Directory.Exists(ProjectSet.ExportDirectory) Then
            Try
                TrigScriptExportModule.ExportToDirectory(ProjectSet.ExportDirectory)
            Catch
            End Try
        ElseIf ProgramSet.isAutoCompile = True AndAlso ProjectSet.euddraftuse = True Then
            eudplib.Toflie()
        End If
        refreshSet()
    End Sub
    Private Sub 끝내기ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        If ProjectSet.Close() Then
            Me.Close()
        End If
    End Sub

    Private Sub MPQFormOpen(sender As Object, e As EventArgs) Handles Button13.Click
        'MPQForm.Location = Me.Location
        Me.Visible = False
        MPQForm.ShowDialog()
        Me.Visible = True

    End Sub

    Private Sub GRPFormOpen(sender As Object, e As EventArgs) Handles Button8.Click
        'GRPForm.Location = Me.Location
        'Size = New Size(221, 438)
        GRPForm.Size = New Size(806, 438)
        Me.Visible = False
        GRPForm.ShowDialog()
        Me.Visible = True
        nameResetting()
    End Sub

    Private Sub DatEditFormOpen(sender As Object, e As EventArgs) Handles Button2.Click
        'DatEditForm.Location = Me.Location
        DatEditForm.Timer1.Enabled = True

        Me.Visible = False

        DatEditForm.Show()
        My.Forms.DatEditForm.Activate()
    End Sub

    Private Sub FireGraftFormOpen(sender As Object, e As EventArgs) Handles Button3.Click
        Me.Visible = False
        FireGraftForm.Show()
        FireGraftForm.RefreshForm()
        My.Forms.DatEditForm.Activate()
    End Sub

    Private Sub TriggerViewFormOpen(sender As Object, e As EventArgs) Handles Button4.Click
        Me.Visible = False
        TriggerViewerForm.ShowDialog()
        Me.Visible = True
        nameResetting()
    End Sub

    Private Sub plugin_Click(sender As Object, e As EventArgs) Handles Button10.Click
        MsgBox("Plugin editor is disabled in this fork (euddraft-oriented; use TrigScript/epScript export instead).", MsgBoxStyle.Information, ProgramSet.ErrorFormMessage)
    End Sub

    Private Sub FileManager_Click(sender As Object, e As EventArgs) Handles Button15.Click
        ProjectSet.saveStatus = False

        Me.Visible = False
        FileManagerForm.ShowDialog()
        Me.Visible = True
        nameResetting()
        LoadFileimportable()
        DatEditForm.Loadstattxt()
    End Sub


    Private Sub binEditor_Click(sender As Object, e As EventArgs) Handles Button5.Click
        ProjectSet.saveStatus = False

        Me.Visible = False
        binEditorForm.ShowDialog()
        Me.Visible = True
        nameResetting()
    End Sub


    Private Sub TileSet_Click(sender As Object, e As EventArgs) Handles Button6.Click
        MsgBox("TileSet editor is disabled in this fork (incompatible with TrigScript export).", MsgBoxStyle.Information, ProgramSet.ErrorFormMessage)
    End Sub


    Private Sub DebugFormOpen(sender As Object, e As EventArgs) Handles Button12.Click

        Me.Visible = False
        DebugForm.ShowDialog()
        Me.Visible = True
    End Sub



    Private Sub btn_close_Click(sender As Object, e As EventArgs) Handles btn_close.Click
        프로젝트닫기()
    End Sub
    Private Sub Btn_Save_Click(sender As Object, e As EventArgs) Handles btn_Save.Click
        저장()
    End Sub
    Private Sub Btn_OpenFile_Click(sender As Object, e As EventArgs) Handles btn_OpenFile.Click
        열기()
    End Sub
    Private Sub Btn_NewFile_Click(sender As Object, e As EventArgs) Handles btn_NewFile.Click
        새로만들기()
    End Sub


    Private Sub ExportTrigScriptToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportTrigScriptToolStripMenuItem.Click
        TrigScriptExportModule.TryExportWithFolderDialog(Me)
    End Sub

    Private Sub 맵에삽입_Click(sender As Object, e As EventArgs) Handles Button14.Click
        TrigScriptExportModule.TryExportWithFolderDialog(Me)
    End Sub

    Private Sub 도구TToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToolTToolStripMenuItem.DropDownOpened
        ExportTrigScriptToolStripMenuItem.Enabled = ProjectSet.isload
        SyncExportFormatMenu()
    End Sub

    Private Sub 블로그설명서ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BlogHelpToolStripMenuItem.Click
        Try
            Process.Start("https://github.com/michaelcrusso/ee2-exporter")
        Catch ex As Exception
            MsgBox("Could not open browser: " & ex.Message, MsgBoxStyle.Exclamation, ProgramSet.ErrorFormMessage)
        End Try
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        ProjectSet.saveStatus = False

        If ProjectSet.LoadFromCHK = False Then
            MsgBox(Lan.GetText(Me.Name, "CHKMsg"), MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
        Else
            BulidForm.Close()
            Me.Visible = False
            TrigEditorForm.ShowDialog()
            Me.Visible = True
            nameResetting()
        End If
    End Sub

    Public LastData As Date
    Private Sub CheckMapWrite_Tick(sender As Object, e As EventArgs) Handles CheckMapWrite.Tick
        ' Map watch is only meaningful when an InputMap is set.
        If ProjectSet.isload = True AndAlso ProjectSet.InputMap <> "" AndAlso CheckFileExist(ProjectSet.InputMap) = False Then
            Dim fileinfo As New FileInfo(ProjectSet.InputMap)
            If LastData < fileinfo.LastWriteTime Then
                LastData = fileinfo.LastWriteTime
                ProjectSet.LoadCHKdata()
                If ProgramSet.isAutoCompile = True AndAlso ProjectSet.ExportDirectory <> "" AndAlso Directory.Exists(ProjectSet.ExportDirectory) Then
                    Try
                        TrigScriptExportModule.ExportToDirectory(ProjectSet.ExportDirectory)
                    Catch
                    End Try
                End If
            End If
        End If
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        ProjectSet.saveStatus = False

        Me.Visible = False
        SoundPlayerForm.ShowDialog()
        Me.Visible = True
        nameResetting()
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        Me.Visible = False
        FileSettingForm.ShowDialog()
        Me.Visible = True
        nameResetting()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        ProgramSet.isAutoCompile = CheckBox1.Checked
    End Sub

    Private Sub RecentFileMenuItem_Click(sender As ToolStripMenuItem, e As EventArgs)
        Dim fileName = sender.Text
        If ProjectSet.Close() = True Then
            ProjectSet.Load(fileName)
            RememberProjectDirectory(fileName)
            If ProjectSet.ExportDirectory <> "" Then
                RememberExportDirectory(ProjectSet.ExportDirectory)
            End If
            CheckMapFile()
            If ProjectSet.InputMap <> "" AndAlso CheckFileExist(ProjectSet.InputMap) = False Then
                Dim fileinfo As New FileInfo(ProjectSet.InputMap)
                LastData = fileinfo.LastWriteTime
            End If
        End If
        refreshSet()
    End Sub
    Public Sub SaveRecentFile(strPath As String)
        OpenRecentToolStripMenuItem.DropDownItems.Clear()
        LoadRecentList()
        If (RecentlyOpenedFiles.Contains(strPath)) Then
            RecentlyOpenedFiles.Remove(strPath)
        End If
        RecentlyOpenedFiles.Add(strPath)
        While RecentlyOpenedFiles.Count > 15
            RecentlyOpenedFiles.RemoveAt(0)
        End While
        Dim stringToWrite As New StreamWriter(System.Environment.CurrentDirectory + "\Recent.txt")
        For Each item As String In RecentlyOpenedFiles
            stringToWrite.WriteLine(item)
        Next
        stringToWrite.Flush()
        stringToWrite.Close()
    End Sub

    Private Sub LoadRecentList()
        RecentlyOpenedFiles.Clear()
        Try
            Dim srStream As New StreamReader(System.Environment.CurrentDirectory + "\Recent.txt")
            Dim strLine As String = ""
            While (InlineAssignHelper(strLine, srStream.ReadLine())) IsNot Nothing
                RecentlyOpenedFiles.Add(strLine)
            End While
            srStream.Close()
        Catch ex As Exception
        End Try
        RecentlyOpenedFiles.Reverse()
        OpenRecentToolStripMenuItem.Visible = RecentlyOpenedFiles.Count > 0
    End Sub

    Public Sub RefreshRecentlyOpenedList()
        LoadRecentList()
        For Each item As String In RecentlyOpenedFiles
            Dim fileRecent As New ToolStripMenuItem(item, Nothing, New EventHandler(AddressOf RecentFileMenuItem_Click))
            OpenRecentToolStripMenuItem.DropDownItems.Add(fileRecent)
        Next
    End Sub

    Private Shared Function InlineAssignHelper(Of T) _
          (ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function

    Private Sub ThemeSettingsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ThemeSettingsToolStripMenuItem.Click
        ThemeSetForm.ShowDialog()
        SaveTheme()
        refreshSet()
    End Sub
End Class

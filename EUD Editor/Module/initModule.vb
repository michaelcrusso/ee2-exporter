Imports System.IO
Imports Microsoft.Win32

Module initModule
    Public DirectoryPath As String

    ''' <summary>
    ''' Walk up from the exe folder looking for a sibling/ancestor "mpqs" directory
    ''' (repo layout: ee2-exporter/mpqs next to "EUD Editor").
    ''' </summary>
    Private Function FindRepoMpqDirectory() As String
        Try
            Dim dir As DirectoryInfo = New DirectoryInfo(My.Application.Info.DirectoryPath)
            While dir IsNot Nothing
                Dim candidate As String = Path.Combine(dir.FullName, "mpqs")
                If Directory.Exists(candidate) Then
                    Return candidate
                End If
                dir = dir.Parent
            End While
        Catch
        End Try
        Return ""
    End Function

    ''' <summary>
    ''' Case-insensitive MPQ lookup (Linux/Wine Z: paths are case-sensitive).
    ''' </summary>
    Private Function FindMpqFile(directoryPath As String, fileName As String) As String
        If directoryPath = "" OrElse Directory.Exists(directoryPath) = False Then
            Return ""
        End If
        Dim exact As String = Path.Combine(directoryPath, fileName)
        If File.Exists(exact) Then
            Return exact
        End If
        Try
            For Each pathEntry As String In Directory.GetFiles(directoryPath)
                If String.Equals(Path.GetFileName(pathEntry), fileName, StringComparison.OrdinalIgnoreCase) Then
                    Return pathEntry
                End If
            Next
        Catch
        End Try
        Return ""
    End Function

    Public Sub PersistMpqSettings()
        PersistProgramSettings()
    End Sub

    ''' <summary>
    ''' Save StarCraft / euddraft / MPQ paths immediately so a crash or unclean exit
    ''' does not force re-picking them on the next launch.
    ''' </summary>
    Public Sub PersistProgramSettings()
        My.Settings.StarDirec = ProgramSet.StarDirec
        My.Settings.euddraftDirec = ProgramSet.euddraftDirec
        My.Settings.StarVersion = ProgramSet.StarVersion
        My.Settings.mpqDirec = String.Join(",", ProgramSet.DatMPQDirec)
        My.Settings.Save()
    End Sub

    ''' <summary>First existing directory among candidates, else process cwd if usable.</summary>
    Public Function PreferExistingDirectory(ParamArray candidates As String()) As String
        For Each candidate As String In candidates
            If Not String.IsNullOrWhiteSpace(candidate) Then
                Try
                    If Directory.Exists(candidate) Then
                        Return Path.GetFullPath(candidate)
                    End If
                Catch
                End Try
            End If
        Next
        Try
            Dim cwd As String = Environment.CurrentDirectory
            If Directory.Exists(cwd) Then
                Return Path.GetFullPath(cwd)
            End If
        Catch
        End Try
        Return ""
    End Function

    Public Sub RememberProjectDirectory(filePath As String)
        If String.IsNullOrWhiteSpace(filePath) Then
            Return
        End If
        Try
            Dim directoryPath As String = Path.GetDirectoryName(filePath)
            If Not String.IsNullOrWhiteSpace(directoryPath) AndAlso Directory.Exists(directoryPath) Then
                My.Settings.LastProjectDirectory = directoryPath
                My.Settings.Save()
            End If
        Catch
        End Try
    End Sub

    Public Sub RememberExportDirectory(directoryPath As String)
        If String.IsNullOrWhiteSpace(directoryPath) Then
            Return
        End If
        Try
            If Directory.Exists(directoryPath) Then
                My.Settings.LastExportDirectory = directoryPath
                ProjectSet.ExportDirectory = directoryPath
                My.Settings.Save()
            End If
        Catch
        End Try
    End Sub

    Public Function ResolveProjectDialogDirectory() As String
        Dim fromProject As String = ""
        If Not String.IsNullOrWhiteSpace(ProjectSet.filename) Then
            Try
                fromProject = Path.GetDirectoryName(ProjectSet.filename)
            Catch
            End Try
        End If
        Return PreferExistingDirectory(fromProject, My.Settings.LastProjectDirectory, My.Settings.LastExportDirectory)
    End Function

    Public Function ResolveExportDialogDirectory() As String
        Dim fromProject As String = ""
        If Not String.IsNullOrWhiteSpace(ProjectSet.filename) Then
            Try
                fromProject = Path.GetDirectoryName(ProjectSet.filename)
            Catch
            End Try
        End If
        Return PreferExistingDirectory(ProjectSet.ExportDirectory, My.Settings.LastExportDirectory, fromProject, My.Settings.LastProjectDirectory)
    End Function

    Sub DeleteFilesFromFolder(Folder As String)
        If Directory.Exists(Folder) Then
            For Each _file As String In Directory.GetFiles(Folder)
                File.Delete(_file)
            Next
            For Each _folder As String In Directory.GetDirectories(Folder)

                DeleteFilesFromFolder(_folder)
            Next
        End If
    End Sub

    Public Function init() As Boolean
        Dim filename As String
        filename = My.Application.Info.DirectoryPath & "\Data\Language\한국어(Korean)"
        If System.IO.Directory.Exists(filename) Then
            DeleteFilesFromFolder(filename)
            System.IO.Directory.Delete(filename)
        End If
        filename = My.Application.Info.DirectoryPath & "\Data\Language\English(English)"
        If System.IO.Directory.Exists(filename) Then
            DeleteFilesFromFolder(filename)
            System.IO.Directory.Delete(filename)
        End If
        filename = My.Application.Info.DirectoryPath & "\TE함수"
        If System.IO.Directory.Exists(filename) Then
            DeleteFilesFromFolder(filename)
            System.IO.Directory.Delete(filename)
        End If

        filename = My.Application.Info.DirectoryPath & "\Data\Language\" & My.Settings.Language
        If System.IO.Directory.Exists(filename) = False Then
            My.Settings.Language = "English"
        End If

        Dim hyperLink As Boolean = False
        Dim testmode As Boolean = False
        Dim mainControlMode As Boolean = False
        'My.Settings.Reset()

        If mainControlMode Then
            Dim data As String
            Try
                With CreateObject("WinHttp.WinHttpRequest.5.1")
                    .Open("GET", "http://blog.naver.com/PostView.nhn?blogId=sksljh2091&logNo=220883526276")
                    .Send
                    .WaitForResponse

                    data = .ResponseText

                    Dim Searcher As String = ProgramSet.Version
                    Dim ttempstr As String = Mid(data, InStr(data, Searcher) + 1)
                    'Mid(data, InStr(data, "euddraftversion"))

                    Dim currentversion As String = Mid(ttempstr, Searcher.Length, InStr(ttempstr, "]") - Searcher.Length)


                    If currentversion = False Then
                        MsgBox("사용기간이 지났습니다. 강제로 종료됩니다.", MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
                        End
                    End If
                End With
            Catch ex As Exception
                MsgBox("인터넷 연결이 끊켜 실행 가능 여부를 판단 할 수 없습니다. 강제 종료됩니다.", MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
                End
            End Try
        End If



        'Test======================
        If testmode = True Then
            Try
                Dim lastMonth2 As String = "Jan"
                Dim lastMonth As String = "Feb"
                Dim lastday As Integer = 29
                Dim lastyear As Integer = 2017

                Dim data As String
                With CreateObject("WinHttp.WinHttpRequest.5.1")
                    .Open("GET", "http://map.naver.com/common2/checkNds.nhn")
                    .Send
                    data = Replace$(Replace$(Split(.ResponseText, vbNewLine)(6), vbTab, ""), ". ", "-")
                End With

                Dim monthandday As String = Mid(data, 1, InStr(data, ",") - 1)

                Dim Month As String = monthandday.Split(" ")(0)
                Dim day As String = monthandday.Split(" ")(1)
                Dim year As String = data.Split(" ")(2)
                data = year & "." & Month & "." & day
                MsgBox("현재 날짜는 " & data & "입니다." & vbCrLf & lastyear & "." & lastMonth & "." & lastday & "까지 사용 가능합니다.", MsgBoxStyle.Exclamation, ProgramSet.AlterFormMessage)

                If year > lastyear Or (Month <> lastMonth And Month <> lastMonth2) Or day > lastday Then
                    MsgBox("사용기간이 지났습니다. 강제로 종료됩니다.", MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
                    End
                End If
            Catch ex As Exception
                MsgBox("인터넷 연결이 끊켜 날짜 확인이 불가능 합니다.. 강제로 종료됩니다.", MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
                End
            End Try
        End If
        'Test======================

        If UBound(Diagnostics.Process.GetProcessesByName(Diagnostics.Process.GetCurrentProcess.ProcessName)) > 0 Then
            MsgBox(Lan.GetMsgText("notoverStart"), MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, ProgramSet.ErrorFormMessage)
            End
        End If



        If ProgramSet.Version.Contains("TEST") Then
            MsgBox(Lan.GetText("Msgbox", "TESTAlter"), MsgBoxStyle.Exclamation, ProgramSet.AlterFormMessage)
        End If




        'System.IO.Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\Data\temp")



        ProgramSet.StarDirec = My.Settings.StarDirec
        ProgramSet.euddraftDirec = My.Settings.euddraftDirec
        ProgramSet.StarVersion = My.Settings.StarVersion
        ProgramSet.isAutoCompile = My.Settings.AutoCompile

        If My.Settings.mpqDirec.Split(",").Count = 4 Then
            ProgramSet.DatMPQDirec = My.Settings.mpqDirec.Split(",")
        End If


        If My.Settings.DatEditColor1 = Nothing Then
            ProgramSet.colorBackground = ThemeSetForm.darkModeColorBackground
            ProgramSet.colorLabelText = ThemeSetForm.darkModeColorLabelText
            ProgramSet.colorFieldBackground = ThemeSetForm.darkModeColorFieldBackground
            ProgramSet.colorFieldText = ThemeSetForm.darkModeColorFieldText
            ProgramSet.colorCodeBackground = ThemeSetForm.darkModeColorCodeBackground
            ProgramSet.colorPanelBackground = ThemeSetForm.darkModeColorPanelBackground
            ProgramSet.colorChangedBackground = ThemeSetForm.darkModeColorChangedBackground
            ProgramSet.colorCheckedBackground = ThemeSetForm.darkModeColorCheckedBackground
        Else
            ProgramSet.colorFieldText = My.Settings.DatEditColor1
            ProgramSet.colorFieldBackground = My.Settings.DatEditColor2
            ProgramSet.colorChangedBackground = My.Settings.DatEditColor3
            ProgramSet.colorCheckedBackground = My.Settings.DatEditColor4
            ProgramSet.colorChangedBackground = My.Settings.DatEditColor5
            ProgramSet.colorCheckedBackground = My.Settings.DatEditColor6
            ProgramSet.colorCodeBackground = My.Settings.DatEditColor7
            ProgramSet.colorPanelBackground = My.Settings.DatEditColor8

        End If


        ' StarCraft.exe is still required (MPQ / Casc game data roots).
        ' euddraft.exe is optional in this fork (TrigScript export replaces that path).
        If ProgramSet.StarDirec = "" Then
            SettingForm.StartPosition = FormStartPosition.CenterScreen
            SettingForm.TopMost = True
            SettingForm.PreSizeSet()
            SettingForm.ComboBox1.SelectedIndex = 1
            If SettingForm.ShowDialog() = DialogResult.Cancel Then
                Return False
            End If
            SettingForm.StartPosition = FormStartPosition.CenterParent
            SettingForm.TopMost = False
        End If


        Dim DatMPQ() As String = {"Patch_rt.mpq", "BrooDat.mpq", "BroodWar.mpq", "StarDat.mpq"}
        Dim starCraftMpqDir As String = ""
        If ProgramSet.StarDirec <> "" Then
            starCraftMpqDir = Path.GetDirectoryName(ProgramSet.StarDirec)
        End If
        Dim repoMpqDir As String = FindRepoMpqDirectory()

        ' Resolve missing MPQs: saved path → repo mpqs/ → next to StarCraft.exe
        For i = 0 To 3
            If CheckFileExist(ProgramSet.DatMPQDirec(i)) = True Then
                Dim found As String = FindMpqFile(repoMpqDir, DatMPQ(i))
                If found = "" Then
                    found = FindMpqFile(starCraftMpqDir, DatMPQ(i))
                End If
                If found <> "" Then
                    ProgramSet.DatMPQDirec(i) = found
                End If
            End If
        Next

        Dim needMpqPrompt As Boolean = False
        For i = 0 To DatMPQ.Count - 1
            If CheckFileExist(ProgramSet.DatMPQDirec(i)) = True Then
                needMpqPrompt = True
                Exit For
            End If
        Next
        If needMpqPrompt Then
            MsgBox(Lan.GetMsgText("enMPQ"), MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
            If SetMPQForm.ShowDialog() = DialogResult.OK Then
                PersistMpqSettings()
            End If
        Else
            ' Keep Wine/user settings in sync when we auto-resolved from mpqs/
            PersistMpqSettings()
        End If


        If My.User.IsInRole("administrators") = False Then
            ' MsgBox(Lan.GetMsgText("notAdmin"), MsgBoxStyle.Critical, ProgramSet.ErrorFormMessage)
            'End
        End If



        '데이터 읽어오기
        DataLoad()




        DirectoryPath = My.Application.Info.DirectoryPath '실행파일 위치
        If My.Application.CommandLineArgs.Count <> 0 Then '연결로 실행되었을 경우.
            ProjectSet.Reset()
            My.Forms.SettingForm.StartPosition = FormStartPosition.CenterScreen
            ProjectSet.Load(My.Application.CommandLineArgs(0))
            'MsgBox(My.Application.CommandLineArgs(0))
            My.Forms.SettingForm.StartPosition = FormStartPosition.CenterParent
            CheckMapFile()
        Else
            If hyperLink Then
                ProjectSet.Reset()
                My.Forms.SettingForm.StartPosition = FormStartPosition.CenterScreen
                ProjectSet.Load("C:\Users\skslj\Desktop\EUD에디터Save1.e2s") 'C:\Users\skslj\Desktop\EUD에디터Save1.e2s") '"C:\Users\skslj\Desktop\EUD에디터Save1.e2s")
                My.Forms.SettingForm.StartPosition = FormStartPosition.CenterParent
                CheckMapFile()
            End If
        End If




        DataLoadAfterProgramLoad()

        'Test======================
        'If testmode = True Then

        ' End If
        'Test======================


        'OutPutEUDTriggers()

        '

        ' File associations are nice-to-have on Windows; under Wine the Classes key
        ' may be missing and OpenSubKey returns Nothing. Never block startup for this.
        Try
            Dim classes As RegistryKey = My.Computer.Registry.CurrentUser.OpenSubKey(
                "Software\Classes", RegistryKeyPermissionCheck.ReadWriteSubTree)
            If classes Is Nothing Then
                classes = My.Computer.Registry.CurrentUser.CreateSubKey("Software\Classes")
            End If
            If classes IsNot Nothing Then
                classes.CreateSubKey(".e2s").SetValue("",
                  "e2s", Microsoft.Win32.RegistryValueKind.String)
                classes.CreateSubKey("e2s\shell\open\command").SetValue("",
                Application.ExecutablePath & " ""%l"" ", Microsoft.Win32.RegistryValueKind.String)
                classes.CreateSubKey("e2s\DefaultIcon").SetValue("",
               My.Application.Info.DirectoryPath & "\Data\icons\e2s.ico" & ",0", Microsoft.Win32.RegistryValueKind.String)


                classes.CreateSubKey(".ees").SetValue("",
                  "ees", Microsoft.Win32.RegistryValueKind.String)
                classes.CreateSubKey("ees\shell\open\command").SetValue("",
                Application.ExecutablePath & " ""%l"" ", Microsoft.Win32.RegistryValueKind.String)
                classes.CreateSubKey("ees\DefaultIcon").SetValue("",
                My.Application.Info.DirectoryPath & "\Data\icons\ees.ico" & ",0", Microsoft.Win32.RegistryValueKind.String)


                classes.CreateSubKey(".mem").SetValue("",
                  "mem", Microsoft.Win32.RegistryValueKind.String)
                classes.CreateSubKey("mem\shell\open\command").SetValue("",
                Application.ExecutablePath & " ""%l"" ", Microsoft.Win32.RegistryValueKind.String)
                classes.CreateSubKey("mem\DefaultIcon").SetValue("",
               My.Application.Info.DirectoryPath & "\Data\icons\mem.ico" & ",0", Microsoft.Win32.RegistryValueKind.String)


                classes.CreateSubKey(".e2p").SetValue("",
                  "e2p", Microsoft.Win32.RegistryValueKind.String)
                classes.CreateSubKey("e2p\shell\open\command").SetValue("",
                Application.ExecutablePath & " ""%l"" ", Microsoft.Win32.RegistryValueKind.String)
                classes.CreateSubKey("e2p\DefaultIcon").SetValue("",
               My.Application.Info.DirectoryPath & "\Data\icons\e2p.ico" & ",0", Microsoft.Win32.RegistryValueKind.String)

                classes.Close()
            End If
        Catch
        End Try

        'TextEditor.ShowDialog()
        'Main.Close()

        If My.Settings.IgnoreUpdate = False And CheckUpdateAble() = True Then
            CheckUpdateForm.ShowDialog()
        End If

        Return True
    End Function
End Module

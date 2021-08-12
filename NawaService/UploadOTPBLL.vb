Imports System.Threading
Imports System.Data.SqlClient
Imports System.Data
Imports System.Data.Entity
Imports System.Windows.Forms
Imports System.Security.Cryptography
Imports System.IO
Imports System.Configuration

Public Class UploadOTPBLL
    Implements IDisposable

    Private mylog As New NawaConsoleLog


    Public Enum EnumSFTPSetting
        'HostName = 5001
        'UserName = 5002
        'Password = 5003
        'SshHostKeyFingerprint = 5005
        'PortNumber = 5004
        'PathFolderFT = 5006
        'PathFolderDC = 5007
        'CekSFTP = 5000
        'LocalFolderPath = 5008
        'sessionname = 6000
        CutoffBlokirNonBDI = 4002
        CutoffOpenNonBDI = 4005

        CekSFTP = 5000
        NCBSInFolderPath = 5001
        NCBSInBackupFolderPath = 5002
        NCBSInFailedFolderPath = 5003
        NCBSOutFolderPath = 5004
        NCBSOutBackupFolderPath = 5005
        NCBSOutFailedFolderPath = 5006
        LocalFolderPath = 5008
        TransferFileCommand = 5009
        SFTPUserId = 5010
        SFTPIPDestination = 5011
        NCBSKeyFolderPath = 5012
        NCBSKeyName = 5013
        BatchFileCommand = 5014
        AESFileDirectory = 5015
        AESPassword = 5016
    End Enum

    Sub run()

        Dim lngFk_EODTaskDetailLog_ID As Long = 0
        Try
            While True
                Thread.Sleep(My.Settings.IntThreadInterval)

                'Re Generate Disbursement File -- jika ada data terbaru dengan filetype 1 dan status pengiriman 13 maka Generate Disbursement File 
                'Using objDb As NawaDataEntities = New NawaDataEntities
                '    Dim objTrxOTP_NCBSFile As List(Of TrxOTP_NCBSFile) =
                '        (From a In objDb.TrxOTP_NCBSFile
                '         Join b In (From x In objDb.TrxOTP_NCBSFile
                '                    Where x.Fk_MsNCBSFileType_Id = 1
                '                    Group x By Keys = New With {Key x.Fk_TrxDafnom_OTP_Id}
                '                       Into Group
                '                    Select New With {
                '                           .Fk_TrxDafnom_OTP_Id = Keys.Fk_TrxDafnom_OTP_Id,
                '                           .Pk_TrxOTP_NCBSFile_Id = Group.Max(Function(y) y.Pk_TrxOTP_NCBSFile_Id)
                '                           }
                '                   ) On New With {a.Pk_TrxOTP_NCBSFile_Id, a.Fk_TrxDafnom_OTP_Id} Equals New With {b.Pk_TrxOTP_NCBSFile_Id, b.Fk_TrxDafnom_OTP_Id}
                '         Where a.Fk_MsStatusPengiriman_Id = 13 And a.Fk_MsNCBSFileType_Id = 1
                '         Select a).ToList
                '    For Each item As TrxOTP_NCBSFile In objTrxOTP_NCBSFile
                '        Dim lngPk_HoldFundFileId As Long = ReGenerateFileDisbursement(item.Fk_TrxDafnom_OTP_Id.GetValueOrDefault)
                '        UploadFile(lngPk_HoldFundFileId)
                '    Next

                'End Using

                'Disbursement
                Using objDb As NawaDataEntities = New NawaDataEntities
                    Dim objTrxOTP_NCBSFile As List(Of TrxOTP_NCBSFile) =
                        objDb.TrxOTP_NCBSFile.Where(Function(x) x.Fk_MsStatusPengiriman_Id = 1).ToList
                    If objTrxOTP_NCBSFile.Count > 0 Then
                        Dim item As TrxOTP_NCBSFile = objTrxOTP_NCBSFile(0)
                        UploadFile(item.Pk_TrxOTP_NCBSFile_Id)
                    End If
                End Using

                'Cek Balikan Disbursement
                'Kalo ada, update status ke Table TrxOTP_NCBSFile
                CekNCBSResponse()

                '1. Unhold -- process unhold data yang terhold dengan sukses pada hari berikutnya dengan time lebih besar dari time yg di tentukan
                '2. Reprocess data unhold yang gagal
                Using objDb As NawaDataEntities = New NawaDataEntities
                    Dim objTrxOTP_NCBSFile As List(Of TrxOTP_NCBSFile) =
                       objDb.TrxOTP_NCBSFile.Where(Function(x) x.Fk_MsStatusPengiriman_Id = 14).ToList
                    '.Union(
                    '    From a In objDb.TrxOTP_NCBSFile
                    '    Join b In (From x In objDb.TrxOTP_NCBSFile
                    '               Where x.Fk_MsNCBSFileType_Id = 5
                    '               Group x By Keys = New With {Key x.Fk_TrxDafnom_OTP_Id}
                    '                       Into Group
                    '               Select New With {
                    '                           .Fk_TrxDafnom_OTP_Id = Keys.Fk_TrxDafnom_OTP_Id,
                    '                           .Pk_TrxOTP_NCBSFile_Id = Group.Max(Function(y) y.Pk_TrxOTP_NCBSFile_Id)
                    '                           }
                    '              ) On New With {a.Pk_TrxOTP_NCBSFile_Id, a.Fk_TrxDafnom_OTP_Id} Equals New With {b.Pk_TrxOTP_NCBSFile_Id, b.Fk_TrxDafnom_OTP_Id}
                    '    Where a.Fk_MsStatusPengiriman_Id = 13 And (a.Fk_MsNCBSFileType_Id = 5)
                    '    Select a
                    ').ToList
                    For Each item As TrxOTP_NCBSFile In objTrxOTP_NCBSFile
                        If item.GeneratedDate.Value.Date < Date.Now.Date Then
                            Dim strJamCutoffOpen As String = GetSFTPSettingValue(EnumSFTPSetting.CutoffOpenNonBDI)
                            If IsNumeric(strJamCutoffOpen) Then
                                Dim intCurrentHour As Integer = DatePart(DateInterval.Hour, Date.Now)
                                If intCurrentHour >= CInt(strJamCutoffOpen) Then
                                    'unHold Fund
                                    Dim lngPk_HoldFundFileId As Long = GenerateFile(item.Fk_TrxDafnom_OTP_Id, item.Pk_TrxOTP_NCBSFile_Id, "Usp_GenerateUnHoldFund_SAFV", "Usp_GetFileNameUnHoldFund", EnumMsNCBSFileType.UnholdFund)
                                    If lngPk_HoldFundFileId > 0 Then
                                        UploadFile(lngPk_HoldFundFileId)
                                    End If
                                End If
                            End If
                        End If
                    Next
                End Using

                'GEFU & Hold - process semua data yang 
                ' 1. NCBS Success 
                ' 2. GEFU yang sudah ter Unhold 
                ' 3. data terbaru dengan status 13(Requet Resent) dan MsNCBSFileType 2(GEFU BDI) atau 3(GEFU Non BDI)
                Using objDb As NawaDataEntities = New NawaDataEntities
                    Dim objTrxOTP_NCBSFile As List(Of TrxOTP_NCBSFile) =
                        objDb.TrxOTP_NCBSFile.Where(Function(x) x.Fk_MsStatusPengiriman_Id = 4).Union(
                        objDb.TrxOTP_NCBSFile.Where(Function(x) x.Fk_MsStatusPengiriman_Id = 15)
                        ).Union(
                        From a In objDb.TrxOTP_NCBSFile
                        Join b In (From x In objDb.TrxOTP_NCBSFile
                                   Where x.Fk_MsNCBSFileType_Id = 2 Or x.Fk_MsNCBSFileType_Id = 3
                                   Group x By Keys = New With {Key x.Fk_TrxDafnom_OTP_Id}
                                           Into Group
                                   Select New With {
                                               .Fk_TrxDafnom_OTP_Id = Keys.Fk_TrxDafnom_OTP_Id,
                                               .Pk_TrxOTP_NCBSFile_Id = Group.Max(Function(y) y.Pk_TrxOTP_NCBSFile_Id)
                                               }
                                  ) On New With {a.Pk_TrxOTP_NCBSFile_Id, a.Fk_TrxDafnom_OTP_Id} Equals New With {b.Pk_TrxOTP_NCBSFile_Id, b.Fk_TrxDafnom_OTP_Id}
                        Where a.Fk_MsStatusPengiriman_Id = 13 And (a.Fk_MsNCBSFileType_Id = 2 Or a.Fk_MsNCBSFileType_Id = 3)
                        Select a
                    ).ToList

                    If objTrxOTP_NCBSFile.Count > 0 Then
                        Dim item As TrxOTP_NCBSFile = objTrxOTP_NCBSFile(0)

                        'GEFU BDI - hanya dijalankan jika ms sttus pengiriman 4 atau (status pengiriman 13 dan MsNCBSFileType = 2)
                        If item.Fk_MsStatusPengiriman_Id = 4 Or (item.Fk_MsStatusPengiriman_Id = 13 And item.Fk_MsNCBSFileType_Id = 2) Then
                            Try

                                Dim lngPk_GEFUFileId As Long = GenerateFile(item.Fk_TrxDafnom_OTP_Id, item.Pk_TrxOTP_NCBSFile_Id, "Usp_GenerateGEFU_BDI_SAFV", "Usp_GetFileNameGEFU_BDIDisbursement", EnumMsNCBSFileType.GEFUBDI)
                                If lngPk_GEFUFileId > 0 Then
                                    UploadFile(lngPk_GEFUFileId)
                                End If
                            Catch ex As Exception
                                mylog.LogError("An Error has been occurred on Generate GEFU BDI Id: " & item.Fk_TrxDafnom_OTP_Id.ToString, ex)
                            End Try
                        End If



                        ' GEFU NON BDI - hanya dijalankan jika ms sttus pengiriman 4 atau (status pengiriman 13 dan MsNCBSFileType = 3) atau unhold
                        If item.Fk_MsStatusPengiriman_Id = 4 Or (item.Fk_MsStatusPengiriman_Id = 13 And item.Fk_MsNCBSFileType_Id = 3) Or item.Fk_MsStatusPengiriman_Id = 15 Then
                            Dim strJamCutoff As String = GetSFTPSettingValue(EnumSFTPSetting.CutoffBlokirNonBDI)
                            If IsNumeric(strJamCutoff) Then
                                Dim intCurrentHour As Integer = DatePart(DateInterval.Hour, Date.Now)

                                'Jika jam sekarang > jam Cutoff GEFU Non BDI, maka generate Hold
                                'Else, generate GEFU Non BDI
                                If intCurrentHour <= CInt(strJamCutoff) Then
                                    Try
                                        'GEFU Non BDI

                                        Dim lngPk_GEFUNonBDIFileId As Long = GenerateFile(item.Fk_TrxDafnom_OTP_Id, item.Pk_TrxOTP_NCBSFile_Id, "Usp_GenerateGEFU_NON_BDI_SAFV", "Usp_GetFileNameGEFU_Non_BDIDisbursement", EnumMsNCBSFileType.GEFUNonBDI)
                                        If lngPk_GEFUNonBDIFileId > 0 Then
                                            UploadFile(lngPk_GEFUNonBDIFileId)
                                        End If
                                    Catch ex As Exception
                                        mylog.LogError("An Error has been occurred on Generate GEFU Non BDI Id: " & item.Fk_TrxDafnom_OTP_Id.ToString, ex)
                                    End Try
                                Else
                                    Try
                                        'Hold Fund
                                        Dim lngPk_HoldFundFileId As Long = GenerateFile(item.Fk_TrxDafnom_OTP_Id, item.Pk_TrxOTP_NCBSFile_Id, "Usp_GenerateHoldFund_SAFV", "Usp_GetFileNameHoldFund", EnumMsNCBSFileType.HoldFund)
                                        If lngPk_HoldFundFileId > 0 Then
                                            UploadFile(lngPk_HoldFundFileId)
                                        End If
                                    Catch ex As Exception
                                        mylog.LogError("An Error has been occurred on Generate Hold Fund Id: " & item.Fk_TrxDafnom_OTP_Id.ToString, ex)
                                    End Try
                                End If
                            End If
                        End If


                        'Update Status jadi Done
                        'Save Data
                        item.Fk_MsStatusPengiriman_Id = 8
                        objDb.Entry(item).State = Entity.EntityState.Modified
                        objDb.SaveChanges()
                    End If
                End Using

                'Pelunasan
                Using objDb As NawaDataEntities = New NawaDataEntities
                    Dim objTrxOTP_Pelunasan As List(Of TrxOTP_Pelunasan) = objDb.TrxOTP_Pelunasan.Where(Function(x) x.Fk_MsStatusPelunasan_Id = 17).ToList 'OTP Approved by LTS Spv

                    If objTrxOTP_Pelunasan.Count > 0 Then
                        For Each item As TrxOTP_Pelunasan In objTrxOTP_Pelunasan
                            Try
                                'Payoff BDI
                                Dim lngPk_GEFUFileId As Long = GenerateFilePayoff(item.Pk_TrxOTP_Pelunasan_Id, "Usp_GeneratePayoff", "Usp_GetFileNamePayoff", EnumMsNCBSFileType.PartialPayoff)
                                If lngPk_GEFUFileId > 0 Then
                                    Dim no_urut As Long = ReadNoUrut(lngPk_GEFUFileId)
                                    UploadFile(lngPk_GEFUFileId, True)
                                    'Setelah selesai, update Status Pelunasan jadi Waiting NCBS Response
                                    objTrxOTP_Pelunasan(0).No_Urut = no_urut
                                    objTrxOTP_Pelunasan(0).Fk_MsStatusPelunasan_Id = 19
                                    objDb.Entry(objTrxOTP_Pelunasan(0)).State = Entity.EntityState.Modified
                                    objDb.SaveChanges()
                                End If
                            Catch ex As Exception
                                mylog.LogError("An Error has been occurred on Generate Payoff Id: " & item.Pk_TrxOTP_Pelunasan_Id.ToString, ex)
                            End Try
                        Next

                    End If
                End Using

            End While
        Catch ex As Exception
            'Update Status
            Dim strQuery As String = "UPDATE TrxOTP_NCBSFile  SET Fk_MsStatusPengiriman_Id = 3 WHERE Pk_TrxOTP_NCBSFile_Id = " & lngFk_EODTaskDetailLog_ID
            SQLHelper.ExecuteNonQuery(SQLHelper.strConnectionString, CommandType.Text, strQuery, Nothing)

            mylog.LogError("An Error has been occurred on Upload Journal: ", ex)
        End Try
    End Sub
    Sub DecryptFile(sEncryptedfile As String)
        Try
            If sEncryptedfile.IndexOf("Resp") > 0 Then
                Dim strLocalFolderPath As String = GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath)

                Dim strEncryptBatchFileName As String = strLocalFolderPath & "\encrypt.bat"
                Dim strEncryptFileCommand As String = """" & GetSFTPSettingValue(EnumSFTPSetting.AESFileDirectory) & """ """ & sEncryptedfile & """ """ & sEncryptedfile & ".dec"" d " & GetSFTPSettingValue(EnumSFTPSetting.AESPassword)
                System.IO.File.WriteAllText(strEncryptBatchFileName, strEncryptFileCommand)

                Dim resultoutput As String
                Using myProcess As New Process()
                    myProcess.StartInfo.UseShellExecute = False
                    myProcess.StartInfo.FileName = strEncryptBatchFileName
                    myProcess.StartInfo.RedirectStandardOutput = True
                    myProcess.StartInfo.RedirectStandardError = True
                    myProcess.StartInfo.CreateNoWindow = True
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden

                    mylog.LogInfo("Process will start")

                    Dim abc = myProcess.Start()

                    myProcess.WaitForExit(2000)

                    Dim result As String = myProcess.StandardError.ReadToEnd()
                    System.IO.File.AppendAllText(strLocalFolderPath & "\Error.txt", result)

                    If result <> "" Then
                        mylog.LogInfo(result)
                    End If

                    resultoutput = myProcess.StandardOutput.ReadToEnd()
                    System.IO.File.AppendAllText(strLocalFolderPath & "\output.txt", resultoutput)

                    If resultoutput <> "" Then
                        mylog.LogInfo(resultoutput)
                    End If

                    If myProcess.ExitCode = 1 Then
                        Throw New Exception(result)
                    End If

                    myProcess.Close()
                    mylog.LogInfo("Process End")
                End Using

                'Encrypt(strFilePath & ".ori", strFilePath)
                If File.Exists(sEncryptedfile & ".dec") Then
                    'If resultoutput = "" Then
                    Dim strFileName As String = System.IO.Path.GetFileName(sEncryptedfile)
                    File.Copy(sEncryptedfile, GetSFTPSettingValue(EnumSFTPSetting.NCBSInBackupFolderPath) + "\" + strFileName + "-e", True)
                    File.Delete(sEncryptedfile)
                    My.Computer.FileSystem.RenameFile(sEncryptedfile & ".dec", strFileName)
                    'Else
                    '    File.Delete(sEncryptedfile & ".dec")
                    'End If

                End If
            End If
        Catch ex As Exception

        End Try

    End Sub
    Sub CekNCBSResponse()
        Try
            'Encrypt
            Dim strFolderInPath As String = GetSFTPSettingValue(EnumSFTPSetting.NCBSInFolderPath)
            Dim strFolderInFailedPath As String = GetSFTPSettingValue(EnumSFTPSetting.NCBSInFailedFolderPath)
            Dim strFolderInBackupPath As String = GetSFTPSettingValue(EnumSFTPSetting.NCBSInBackupFolderPath)

            Using objDb As NawaDataEntities = New NawaDataEntities
                For Each StrFileNameFull As String In System.IO.Directory.GetFiles(strFolderInPath)
                    Dim strFileName As String = System.IO.Path.GetFileName(StrFileNameFull)
                    DecryptFile(StrFileNameFull)
                    Dim strFileNameCek As String = Replace(strFileName, "Resp", "")

                    'Cek ke Table NCBSFile
                    Dim ObjTrxOTP_NCBSFile As TrxOTP_NCBSFile = objDb.TrxOTP_NCBSFile.Where(Function(x) x.NCBSFileName = strFileNameCek).FirstOrDefault
                    If Not ObjTrxOTP_NCBSFile Is Nothing Then
                        Dim IsError As Boolean = False
                        Dim strErrorMessage As String = ""

                        'Utk format nama file utk menentukan apakah jenis Disbursement, GEFU, atau Non GEFU
                        If strFileNameCek.ToUpper.StartsWith("DISBURSEMENT_") Then
                            With ObjTrxOTP_NCBSFile
                                'Cek apakah file Success atau Error
                                For Each strLine As String In System.IO.File.ReadLines(StrFileNameFull)
                                    Dim strItem() As String = strLine.Split("|")
                                    'Baca bagian Data (Headernya 1)
                                    If strItem(0) = "1" Then
                                        If strItem.Count > 0 Then
                                            'Cek Error Message, di bagian n-1, kalo 99 berarti Error, kalo 0 berarti sukses
                                            Select Case strItem(strItem.Count - 2)
                                                Case "99"
                                                    IsError = True
                                                    .Fk_MsStatusPengiriman_Id = 7   'NCBS Failed
                                                    .ErrorMessage = strLine

                                                    'Kalo Gagal, update Fk_MsStatusOTP_Id nya 13(File Sent to NCBS Failed)
                                                    Dim LngPk_TrxDafnom_OTP_Id As Long = ObjTrxOTP_NCBSFile.Fk_TrxDafnom_OTP_Id
                                                    Dim ObjTrxDafnom_OTP As TrxDafnom_OTP = objDb.TrxDafnom_OTP.Where(Function(x) x.Pk_TrxDafnom_OTP_Id = LngPk_TrxDafnom_OTP_Id).FirstOrDefault
                                                    If Not ObjTrxDafnom_OTP Is Nothing Then
                                                        ObjTrxDafnom_OTP.Fk_MsStatusOTP_Id = 13

                                                        'Save
                                                        objDb.Entry(ObjTrxDafnom_OTP).State = Entity.EntityState.Modified
                                                        objDb.SaveChanges()
                                                    End If
                                                Case "0"
                                                    .Fk_MsStatusPengiriman_Id = 4   'NCBS Success

                                                    Dim strLoanNumber As String = ""
                                                    'Contoh format data:
                                                    '1|OF001824302824323|53|003600098176    |11530916|0|CUSTID:11530916, ACCOUNT ID:003600098176
                                                    'Account Id ada di paling belakang

                                                    Dim strAccountInfo() As String = strItem(strItem.Count - 1).Split(":")
                                                    If strAccountInfo.Count > 0 Then
                                                        strLoanNumber = strAccountInfo(strAccountInfo.Count - 1)
                                                    End If

                                                    'Kalo Sukses, update Loan Number nya ke TrxDafnom_OTP
                                                    Dim LngPk_TrxDafnom_OTP_Id As Long = ObjTrxOTP_NCBSFile.Fk_TrxDafnom_OTP_Id
                                                    Dim ObjTrxDafnom_OTP As TrxDafnom_OTP = objDb.TrxDafnom_OTP.Where(Function(x) x.Pk_TrxDafnom_OTP_Id = LngPk_TrxDafnom_OTP_Id).FirstOrDefault
                                                    If Not ObjTrxDafnom_OTP Is Nothing Then
                                                        ObjTrxDafnom_OTP.AccountNo = strLoanNumber

                                                        'Save
                                                        objDb.Entry(ObjTrxDafnom_OTP).State = Entity.EntityState.Modified
                                                        objDb.SaveChanges()
                                                    End If
                                            End Select
                                        End If
                                    End If
                                Next

                                .ResponseDate = Date.Now
                                .ResponseFileName = strFileName
                                .ResponseFileContent = System.IO.File.ReadAllBytes(StrFileNameFull)
                            End With
                            objDb.Entry(ObjTrxOTP_NCBSFile).State = Entity.EntityState.Modified
                            objDb.SaveChanges()
                        ElseIf strFileNameCek.ToUpper.StartsWith("GEFU_BDI_SAFV_") Then
                            With ObjTrxOTP_NCBSFile
                                'Cek apakah file Success atau Error
                                For Each strLine As String In System.IO.File.ReadLines(StrFileNameFull)
                                    If strLine.Length > 0 Then
                                        Select Case Left(strLine, 1)
                                            Case "1"    'Header
                                                'Contoh Format sukses:
                                                '1:20180129:5:
                                                If strLine.Split(":").Length >= 4 Then
                                                    Select Case (strLine.Split(":")(2))
                                                        Case "5"

                                                        Case Else
                                                            IsError = True
                                                            strErrorMessage &= vbCrLf & strLine
                                                    End Select
                                                End If
                                            Case "2"    'Detail
                                                'Contoh Format sukses:
                                                '2:1:003600131522:814:0:INTFCAW:0:1408:944:20180129:C:20180129:IDR:224999999:224999999:1:1:1:TSFR KE IndrO:5:

                                                'Contoh Format gagal:
                                                '2:1:003600134070:814:0:INTFCAW:0:1008:944:20180129:D:20180129:IDR:224999999:224999999:1:1:1:TSFR DR KOPERASI KARYAWAN SATELITE:6:Failure: Account not found - Suspense Entry passed
                                                If strLine.Split(":").Length >= 21 Then
                                                    Select Case (strLine.Split(":")(19))
                                                        Case "5"

                                                        Case Else
                                                            IsError = True
                                                            strErrorMessage &= vbCrLf & strLine

                                                            'Kalo Gagal, update Fk_MsStatusOTP_Id nya 13(File Sent to NCBS Failed)
                                                            Dim LngPk_TrxDafnom_OTP_Id As Long = ObjTrxOTP_NCBSFile.Fk_TrxDafnom_OTP_Id
                                                            Dim ObjTrxDafnom_OTP As TrxDafnom_OTP = objDb.TrxDafnom_OTP.Where(Function(x) x.Pk_TrxDafnom_OTP_Id = LngPk_TrxDafnom_OTP_Id).FirstOrDefault
                                                            If Not ObjTrxDafnom_OTP Is Nothing Then
                                                                ObjTrxDafnom_OTP.Fk_MsStatusOTP_Id = 13

                                                                'Save
                                                                objDb.Entry(ObjTrxDafnom_OTP).State = Entity.EntityState.Modified
                                                                objDb.SaveChanges()
                                                            End If
                                                    End Select
                                                End If
                                            Case "3"    'Footer
                                                If strLine.Split(":").Length >= 7 Then
                                                    Select Case (strLine.Split(":")(5))
                                                        Case "5"

                                                        Case Else
                                                            IsError = True
                                                            strErrorMessage &= vbCrLf & strLine
                                                    End Select
                                                End If
                                        End Select
                                    End If

                                Next

                                If IsError Then
                                    .Fk_MsStatusPengiriman_Id = 9   'GEFU Failed
                                    .ErrorMessage = strErrorMessage
                                Else
                                    .Fk_MsStatusPengiriman_Id = 8   'GEFU Success
                                End If

                                .ResponseDate = Date.Now
                                .ResponseFileName = strFileName
                                .ResponseFileContent = System.IO.File.ReadAllBytes(StrFileNameFull)
                            End With
                            objDb.Entry(ObjTrxOTP_NCBSFile).State = Entity.EntityState.Modified
                            objDb.SaveChanges()
                            objDb.Database.ExecuteSqlCommand("usp_UpdateTrxDafnom_OTP_Pelunasan " + ObjTrxOTP_NCBSFile.Pk_TrxOTP_NCBSFile_Id.ToString())
                        ElseIf strFileNameCek.ToUpper.StartsWith("GEFU_NONBDI_SAFV_") Then
                            With ObjTrxOTP_NCBSFile
                                'Cek apakah file Success atau Error
                                For Each strLine As String In System.IO.File.ReadLines(StrFileNameFull)
                                    If strLine.Length > 0 Then
                                        Select Case Left(strLine, 2)
                                            Case "00"    'Header
                                                'Contoh Format sukses:
                                                '00DOMOLEFT01022018010000000000000008100814200036001264560000000010000000000003396004600 000003396004600 000000000000000 Transfer ke end user KOPERASI Bank Danamon Indonesia        YYP0000000                    S

                                                'Contoh Format gagal 
                                                '00DOMOLEFT31012018010000000000000008100814200036001264560000000010000000000003396004600 000003396004600 000000000000000 Transfer ke end user KOPERASI Bank Danamon Indonesia        YYR                           S
                                                Select Case (strLine.Substring(180, 3))
                                                    Case "YYP"

                                                    Case Else
                                                        IsError = True
                                                        strErrorMessage &= vbCrLf & strLine

                                                        'Kalo Gagal, update Fk_MsStatusOTP_Id nya 13(File Sent to NCBS Failed)
                                                        Dim LngPk_TrxDafnom_OTP_Id As Long = ObjTrxOTP_NCBSFile.Fk_TrxDafnom_OTP_Id
                                                        Dim ObjTrxDafnom_OTP As TrxDafnom_OTP = objDb.TrxDafnom_OTP.Where(Function(x) x.Pk_TrxDafnom_OTP_Id = LngPk_TrxDafnom_OTP_Id).FirstOrDefault
                                                        If Not ObjTrxDafnom_OTP Is Nothing Then
                                                            ObjTrxDafnom_OTP.Fk_MsStatusOTP_Id = 13

                                                            'Save
                                                            objDb.Entry(ObjTrxDafnom_OTP).State = Entity.EntityState.Modified
                                                            objDb.SaveChanges()
                                                        End If
                                                End Select
                                            Case "01"    'Detail
                                                'Contoh Format sukses:
                                                '010000120                              4312312312              Semplak                                                               000001523002300                     0080017MANDIRI                                                                                                  IFT00000YYOUR00081                    Transfer dr KOPERASI KARYAWAN Bank Danamon Indonesia        P0000000

                                                'Contoh Format gagal:
                                                '010000120                              4312312312              Semplak                                                               000001523002300 920                 8      MANDIRI                                                                                                  IFT00000YYOUR00121                    Transfer dr KOPERASI KARYAWAN Bank Danamon Indonesia        R       
                                                Select Case (strLine.Substring(379, 1))
                                                    Case "P"

                                                    Case Else
                                                        IsError = True
                                                        strErrorMessage &= vbCrLf & strLine
                                                End Select

                                        End Select
                                    End If

                                Next

                                If IsError Then
                                    .Fk_MsStatusPengiriman_Id = 9   'GEFU Failed
                                    .ErrorMessage = strErrorMessage
                                Else
                                    .Fk_MsStatusPengiriman_Id = 8   'GEFU Success
                                End If

                                .ResponseDate = Date.Now
                                .ResponseFileName = strFileName
                                .ResponseFileContent = System.IO.File.ReadAllBytes(StrFileNameFull)
                            End With
                            objDb.Entry(ObjTrxOTP_NCBSFile).State = Entity.EntityState.Modified
                            objDb.SaveChanges()
                            objDb.Database.ExecuteSqlCommand("usp_UpdateTrxDafnom_OTP_Pelunasan " + ObjTrxOTP_NCBSFile.Pk_TrxOTP_NCBSFile_Id.ToString())
                        ElseIf strFileNameCek.ToUpper.StartsWith("HOLDFUND_SAFV") Then
                            With ObjTrxOTP_NCBSFile
                                'Cek apakah file Success atau Error
                                For Each strLine As String In System.IO.File.ReadLines(StrFileNameFull)
                                    If strLine.Length > 0 Then
                                        Select Case Left(strLine, 1)

                                            Case "1"    'Detail
                                                'Contoh Format sukses:
                                                '1|25-08-2017|000025119561|54|1|1000000|TESTING|7|6|01-09-2017|0|Success

                                                If strLine.Split("|").Length >= 10 Then
                                                    Select Case (strLine.Split("|")(10))
                                                        Case "0"

                                                        Case Else
                                                            IsError = True
                                                            strErrorMessage &= vbCrLf & strLine.Split("|")(11)
                                                    End Select
                                                End If

                                        End Select
                                    End If

                                Next

                                If IsError Then
                                    .Fk_MsStatusPengiriman_Id = 9   'GEFU Failed
                                    .ErrorMessage = strErrorMessage
                                Else
                                    .Fk_MsStatusPengiriman_Id = 14   'GEFU Hold
                                End If

                                .ResponseDate = Date.Now
                                .ResponseFileName = strFileName
                                .ResponseFileContent = System.IO.File.ReadAllBytes(StrFileNameFull)
                            End With
                            objDb.Entry(ObjTrxOTP_NCBSFile).State = Entity.EntityState.Modified
                            objDb.SaveChanges()
                        ElseIf strFileNameCek.ToUpper.StartsWith("UNHOLDFUND_SAFV") Then
                            With ObjTrxOTP_NCBSFile
                                'Cek apakah file Success atau Error
                                For Each strLine As String In System.IO.File.ReadLines(StrFileNameFull)
                                    If strLine.Length > 0 Then
                                        Select Case Left(strLine, 1)

                                            Case "1"    'Detail
                                                'Contoh Format sukses:
                                                '1|25-08-2017|000025119561|54|1|1000000|TESTING|7|6|01-09-2017|0|Success

                                                If strLine.Split("|").Length >= 10 Then
                                                    Select Case (strLine.Split("|")(10))
                                                        Case "0"

                                                        Case Else
                                                            IsError = True
                                                            strErrorMessage &= vbCrLf & strLine.Split("|")(11)
                                                    End Select
                                                End If

                                        End Select
                                    End If

                                Next

                                If IsError Then
                                    .Fk_MsStatusPengiriman_Id = 9   'GEFU Failed
                                    .ErrorMessage = strErrorMessage
                                Else
                                    .Fk_MsStatusPengiriman_Id = 15   'GEFU Unhold
                                End If

                                .ResponseDate = Date.Now
                                .ResponseFileName = strFileName
                                .ResponseFileContent = System.IO.File.ReadAllBytes(StrFileNameFull)
                            End With
                            objDb.Entry(ObjTrxOTP_NCBSFile).State = Entity.EntityState.Modified
                            objDb.SaveChanges()
                        ElseIf strFileNameCek.ToUpper.StartsWith("SAFVPAYOFF_") Then
                            'ElseIf strFileNameCek.ToUpper.StartsWith("SAFVPREQ_") Then
                            '' 23 Juni 2020 permintaan penggantian nama txt file pelunasan
                            With ObjTrxOTP_NCBSFile
                                'Cek apakah file Success atau Error
                                For Each strLine As String In System.IO.File.ReadLines(StrFileNameFull)
                                    If strLine.Length > 0 Then
                                        Select Case Left(strLine, 1)

                                            Case "1"    'Detail
                                                'Contoh Format sukses:
                                                '1|003571466832    |08-02-2018|3|1|003571460520|100001.00|I|N|308|SAFV-P-308|0|File processing completed successfully

                                                If strLine.Split("|").Length >= 11 Then
                                                    Select Case (strLine.Split("|")(11))
                                                        Case "0"
                                                            Dim no_ncbsfile As Long = strLine.Split("|")(9)
                                                            Dim LngPk_TrxDafnom_OTP_Id As Long = ObjTrxOTP_NCBSFile.Fk_TrxDafnom_OTP_Id
                                                            Dim objTrxPelunasan As TrxOTP_Pelunasan = objDb.TrxOTP_Pelunasan.Where(Function(x) x.Fk_TrxDafnom_OTP_Id = LngPk_TrxDafnom_OTP_Id And x.No_Urut = no_ncbsfile And x.Fk_MsStatusPelunasan_Id = 19).FirstOrDefault
                                                            If Not objTrxPelunasan Is Nothing Then
                                                                objTrxPelunasan.Fk_MsStatusPelunasan_Id = 20
                                                                objTrxPelunasan.Fk_MsStatusDokumen_Id = 2
                                                                'Save
                                                                objDb.Entry(objTrxPelunasan).State = Entity.EntityState.Modified
                                                                objDb.SaveChanges()
                                                            End If
                                                        Case "40"
                                                            Dim no_ncbsfile As Long = strLine.Split("|")(9)
                                                            Dim LngPk_TrxDafnom_OTP_Id As Long = ObjTrxOTP_NCBSFile.Fk_TrxDafnom_OTP_Id
                                                            Dim objTrxPelunasan As TrxOTP_Pelunasan = objDb.TrxOTP_Pelunasan.Where(Function(x) x.Fk_TrxDafnom_OTP_Id = LngPk_TrxDafnom_OTP_Id And x.No_Urut = no_ncbsfile And x.Fk_MsStatusPelunasan_Id = 19).FirstOrDefault
                                                            If Not objTrxPelunasan Is Nothing Then
                                                                objTrxPelunasan.Fk_MsStatusPelunasan_Id = 20
                                                                objTrxPelunasan.Fk_MsStatusDokumen_Id = 2
                                                                'Save
                                                                objDb.Entry(objTrxPelunasan).State = Entity.EntityState.Modified
                                                                objDb.SaveChanges()
                                                            End If

                                                        Case Else
                                                            IsError = True
                                                            strErrorMessage &= vbCrLf & strLine.Split("|")(12)
                                                            Dim no_ncbsfile As Long = strLine.Split("|")(9)
                                                            Dim LngPk_TrxDafnom_OTP_Id As Long = ObjTrxOTP_NCBSFile.Fk_TrxDafnom_OTP_Id
                                                            Dim objTrxPelunasan As TrxOTP_Pelunasan = objDb.TrxOTP_Pelunasan.Where(Function(x) x.Fk_TrxDafnom_OTP_Id = LngPk_TrxDafnom_OTP_Id And x.No_Urut = no_ncbsfile And x.Fk_MsStatusPelunasan_Id = 19).FirstOrDefault
                                                            If Not objTrxPelunasan Is Nothing Then
                                                                objTrxPelunasan.Fk_MsStatusPelunasan_Id = 21
                                                                'Save
                                                                objDb.Entry(objTrxPelunasan).State = Entity.EntityState.Modified
                                                                objDb.SaveChanges()
                                                            End If

                                                    End Select
                                                End If

                                        End Select
                                    End If

                                Next

                                If IsError Then
                                    .Fk_MsStatusPengiriman_Id = 12   'Payoff Failed
                                    .ErrorMessage = strErrorMessage
                                Else
                                    .Fk_MsStatusPengiriman_Id = 11   'Payoff Success
                                End If

                                .ResponseDate = Date.Now
                                .ResponseFileName = strFileName
                                .ResponseFileContent = System.IO.File.ReadAllBytes(StrFileNameFull)
                            End With
                            objDb.Entry(ObjTrxOTP_NCBSFile).State = Entity.EntityState.Modified
                            objDb.SaveChanges()
                        End If

                        If IsError Then
                            'Kalau Error, pindahkan ke folder Failed
                            If System.IO.File.Exists(strFolderInFailedPath & "\" & strFileName) Then
                                System.IO.File.Delete(strFolderInFailedPath & "\" & strFileName)
                            End If
                            System.IO.File.Move(StrFileNameFull, strFolderInFailedPath & "\" & strFileName)
                        Else
                            'Setelah selesai, pindahkan ke folder Backup
                            If System.IO.File.Exists(strFolderInBackupPath & "\" & strFileName) Then
                                System.IO.File.Delete(strFolderInBackupPath & "\" & strFileName)
                            End If
                            System.IO.File.Move(StrFileNameFull, strFolderInBackupPath & "\" & strFileName)
                        End If
                    Else
                        'Kalo di table gak ada dg nama yg sama, pindahkan ke folder Failed
                        If System.IO.File.Exists(strFolderInFailedPath & "\" & strFileName) Then
                            System.IO.File.Delete(strFolderInFailedPath & "\" & strFileName)
                        End If
                        System.IO.File.Move(StrFileNameFull, strFolderInFailedPath & "\" & strFileName)
                    End If

                    'Ambil 1 aja dulu
                    'Cek file berikutnya di thread.sleep berikut
                    Exit For
                Next

            End Using
        Catch ex As Exception
            mylog.LogError("An Error has been occurred on Cek NCBS Response: ", ex)
        End Try
    End Sub

    Sub UploadFile(pk As Integer, Optional ByVal isEncrypted As Boolean = True)
        Dim StrErrorMessage As String = ""
        Try
            Dim strLocalFolderPath As String = GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath)
            Dim strFilePath As String = ""
            Dim filebatch As String = ""

            filebatch = strLocalFolderPath & "\transfer.txt"
            Using objDb As NawaDataEntities = New NawaDataEntities
                Dim objTrxOTP_NCBSFile As TrxOTP_NCBSFile = objDb.TrxOTP_NCBSFile.Where(Function(x) x.Pk_TrxOTP_NCBSFile_Id = pk).FirstOrDefault()

                If Not objTrxOTP_NCBSFile Is Nothing Then
                    StrErrorMessage = "(Id: " & objTrxOTP_NCBSFile.Pk_TrxOTP_NCBSFile_Id.ToString & "; Filename: " & objTrxOTP_NCBSFile.NCBSFileName & ")"
                    mylog.LogInfo("generate file transfer.txt started" & filebatch)

                    System.IO.File.WriteAllText(filebatch, GetSFTPSettingValue(EnumSFTPSetting.TransferFileCommand) & vbCrLf & "put """ & GetSFTPSettingValue(EnumSFTPSetting.NCBSOutFolderPath) & "\" & objTrxOTP_NCBSFile.NCBSFileName & """")
                    mylog.LogInfo("generate file transfer.txt end")

                    strFilePath = GetSFTPSettingValue(EnumSFTPSetting.NCBSOutFolderPath) & "\" & objTrxOTP_NCBSFile.NCBSFileName
                    mylog.LogInfo("generate file ncbs started" & strFilePath)

                    'Cek apakah textfile perlu di encrypt atau tidak
                    If isEncrypted Then
                        System.IO.File.WriteAllBytes(strFilePath & ".ori", objTrxOTP_NCBSFile.NCBSFileContent)

                        Dim strEncryptBatchFileName As String = strLocalFolderPath & "\encrypt.bat"
                        Dim strEncryptFileCommand As String = """" & GetSFTPSettingValue(EnumSFTPSetting.AESFileDirectory) & """ """ & strFilePath & ".ori"" """ & strFilePath & """ e " & GetSFTPSettingValue(EnumSFTPSetting.AESPassword)
                        System.IO.File.WriteAllText(strEncryptBatchFileName, strEncryptFileCommand)



                        Using myProcess As New Process()
                            myProcess.StartInfo.UseShellExecute = False
                            myProcess.StartInfo.FileName = strEncryptBatchFileName
                            myProcess.StartInfo.RedirectStandardOutput = True
                            myProcess.StartInfo.RedirectStandardError = True
                            myProcess.StartInfo.CreateNoWindow = True
                            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden

                            mylog.LogInfo("Process will start")

                            Dim abc = myProcess.Start()

                            myProcess.WaitForExit(2000)

                            Dim result As String = myProcess.StandardError.ReadToEnd()
                            System.IO.File.AppendAllText(strLocalFolderPath & "\Error.txt", result)

                            If result <> "" Then
                                mylog.LogInfo(result)
                            End If

                            Dim resultoutput As String = myProcess.StandardOutput.ReadToEnd()
                            System.IO.File.AppendAllText(strLocalFolderPath & "\output.txt", resultoutput)

                            If resultoutput <> "" Then
                                mylog.LogInfo(resultoutput)
                            End If

                            If myProcess.ExitCode = 1 Then
                                Throw New Exception(result)
                            End If

                            myProcess.Close()
                            mylog.LogInfo("Process End")
                        End Using

                        'Encrypt(strFilePath & ".ori", strFilePath)
                        If File.Exists(strFilePath & ".ori") Then
                            File.Delete(strFilePath & ".ori")
                        End If
                    Else
                        System.IO.File.WriteAllBytes(strFilePath, objTrxOTP_NCBSFile.NCBSFileContent)
                    End If
                    mylog.LogInfo("generate file ncbs End")

                    mylog.LogInfo("Generate File Bat Start")
                    Dim strBatchFileName As String = strLocalFolderPath & "\transfer.bat"
                    Dim strBatchFileCommand As String = GetSFTPSettingValue(EnumSFTPSetting.BatchFileCommand) & vbCrLf &
                        "psftp.exe " & GetSFTPSettingValue(EnumSFTPSetting.SFTPUserId) & "@" & GetSFTPSettingValue(EnumSFTPSetting.SFTPIPDestination) &
                        " -i """ & GetSFTPSettingValue(EnumSFTPSetting.NCBSKeyFolderPath) & "\" & GetSFTPSettingValue(EnumSFTPSetting.NCBSKeyName) &
                        """ -b transfer.txt < yes.txt"
                    System.IO.File.WriteAllText(strBatchFileName, strBatchFileCommand)
                    mylog.LogInfo("Generate File Bat End")

                    mylog.LogInfo("sftp started")


                    Using myProcess As New Process()
                        myProcess.StartInfo.UseShellExecute = False
                        myProcess.StartInfo.FileName = strBatchFileName

                        myProcess.StartInfo.RedirectStandardOutput = True
                        myProcess.StartInfo.RedirectStandardError = True
                        myProcess.StartInfo.CreateNoWindow = True
                        myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden

                        mylog.LogInfo("Process will start")

                        Dim abc = myProcess.Start()

                        mylog.LogInfo("Process started")


                        'Thread.Sleep(2000)

                        mylog.LogInfo("Process wait start")


                        myProcess.WaitForExit(2000)

                        mylog.LogInfo("Process wait End")


                        Dim result As String = myProcess.StandardError.ReadToEnd()
                        System.IO.File.AppendAllText(strLocalFolderPath & "\Error.txt", result)

                        If result <> "" Then
                            mylog.LogInfo(result)
                        End If


                        Dim resultoutput As String = myProcess.StandardOutput.ReadToEnd()
                        System.IO.File.AppendAllText(strLocalFolderPath & "\output.txt", resultoutput)

                        If resultoutput <> "" Then
                            mylog.LogInfo(resultoutput)
                        End If

                        If myProcess.ExitCode = 1 Then
                            Throw New Exception(result)
                        End If


                        myProcess.Close()

                        mylog.LogInfo("Process End")

                        If System.IO.File.Exists(GetSFTPSettingValue(EnumSFTPSetting.NCBSOutBackupFolderPath) & "\" & objTrxOTP_NCBSFile.NCBSFileName) Then
                            System.IO.File.Delete(GetSFTPSettingValue(EnumSFTPSetting.NCBSOutBackupFolderPath) & "\" & objTrxOTP_NCBSFile.NCBSFileName)
                        End If
                        System.IO.File.Move(GetSFTPSettingValue(EnumSFTPSetting.NCBSOutFolderPath) & "\" & objTrxOTP_NCBSFile.NCBSFileName, GetSFTPSettingValue(EnumSFTPSetting.NCBSOutBackupFolderPath) & "\" & objTrxOTP_NCBSFile.NCBSFileName)
                    End Using

                    mylog.LogInfo("sftp End")

                    objTrxOTP_NCBSFile.Fk_MsStatusPengiriman_Id = 2
                    objTrxOTP_NCBSFile.SendDate = Date.Now
                    objDb.Entry(objTrxOTP_NCBSFile).State = Entity.EntityState.Modified
                    objDb.SaveChanges()
                End If
            End Using
        Catch ex As Exception
            mylog.LogError("An Error has been occurred On Upload Journal " & StrErrorMessage & ": ", ex)
        End Try

        'Dim myProcess As New Process()
        'myProcess.StartInfo.UseShellExecute = True
        'myProcess.StartInfo.FileName = strLocalFolderPath & "\" &
        'Dim args1 =
        'myProcess.StartInfo.Arguments = args1
        'myProcess.StartInfo.CreateNoWindow = True
        'Dim abc = myProcess.Start()
        'myProcess.WaitForExit()
    End Sub

    Private Function Assign(Of T)(ByRef source As T, ByVal value As T) As T
        source = value
        Return value
    End Function
    Private Function ReGenerateFileDisbursement(pk_TrxDafnom_OTP_Id As Integer)
        Dim lngReturn As Long = 0

        'Generate file disbursement
        Using ObjDb As New NawaDataEntities
            Dim objTrxDafnom_OTP As TrxDafnom_OTP = ObjDb.TrxDafnom_OTP.Where(Function(x) x.Pk_TrxDafnom_OTP_Id = pk_TrxDafnom_OTP_Id).FirstOrDefault

            Dim strQuery As String = "EXEC Usp_GenerateNCBSDisbursement " & objTrxDafnom_OTP.Pk_TrxDafnom_OTP_Id.ToString
            Dim result As New DataTable
            result = NawaDAL.SQLHelper.ExecuteTable(NawaDAL.SQLHelper.strConnectionString, CommandType.Text, strQuery)

            If result.Rows.Count > 0 Then
                Dim strFileName As String = ""
                strQuery = "EXEC Usp_GetFileNameNCBSDisbursement '" & Date.Now.ToString("yyyyMMdd") & "'"
                Dim dtFileName As Data.DataTable = NawaDAL.SQLHelper.ExecuteTable(NawaDAL.SQLHelper.strConnectionString, CommandType.Text, strQuery)
                If dtFileName.Rows.Count > 0 Then
                    strFileName = dtFileName.Rows(0).Item(0).ToString
                End If

                Dim ObjTrxDafnom As TrxDafnom = ObjDb.TrxDafnoms.Where(Function(x) x.Pk_TrxDafnom_Id = objTrxDafnom_OTP.Fk_TrxDafnom_Id).FirstOrDefault
                If Not ObjTrxDafnom Is Nothing Then
                    Dim ObjTrxLimit As TrxLimit = ObjDb.TrxLimits.Where(Function(x) x.Pk_TrxLimit_Id = ObjTrxDafnom.Fk_TrxLimit_Id).FirstOrDefault
                    If Not ObjTrxLimit Is Nothing Then

                        'Dim strFileName As String = "Disbursement " & ObjTrxLimit.LimitCode & " Batch " & ObjTrxDafnom.Batch & ".txt"

                        'Indra Bu - 5 April 2018:
                        'Dr web cuma menulis file ncbs ke folder sementara.
                        'yg akan mengcopy ke folder NCBS adalah Scheduler
                        Dim strFilePath As String = ConfigurationManager.AppSettings("AssetsTemplates") & strFileName
                        'Dim strFilePath As String = GetSFTPSettingValue(5004) & "\" & strFileName
                        System.IO.File.WriteAllText(strFilePath, result.Rows(0).Item(0).ToString)

                        Dim ObjNCBSFile As New TrxOTP_NCBSFile
                        With ObjNCBSFile
                            .Fk_TrxDafnom_OTP_Id = objTrxDafnom_OTP.Pk_TrxDafnom_OTP_Id
                            .Fk_MsStatusPengiriman_Id = 1
                            .NCBSFileName = strFileName
                            .NCBSFileContent = System.IO.File.ReadAllBytes(strFilePath)
                            .Fk_MsNCBSFileType_Id = 1
                            .GeneratedDate = Date.Now
                        End With
                        ObjDb.Entry(ObjNCBSFile).State = Data.EntityState.Added
                        ObjDb.SaveChanges()
                        lngReturn = ObjNCBSFile.Pk_TrxOTP_NCBSFile_Id
                    End If
                End If
            End If
        End Using
        Return lngReturn


    End Function

    Private Function GenerateFile(ByVal lngFk_TrxDafnom_OTP_Id As Long, ByVal lngPk_TrxOTP_NCBSFile_Id As Long, ByVal strSPGenerate As String, ByVal strSPFilename As String, ByVal ObjFk_MsNCBSFileType_Id As EnumMsNCBSFileType) As Long
        Dim lngReturn As Long = 0
        Dim strLocalFolderPath As String = GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath)
        Using ObjDB As New NawaDataEntities
            Try
                Dim objTrxDafnom_OTP As TrxDafnom_OTP = ObjDB.TrxDafnom_OTP.Where(Function(x) x.Pk_TrxDafnom_OTP_Id = lngFk_TrxDafnom_OTP_Id).FirstOrDefault
                If Not objTrxDafnom_OTP Is Nothing Then
                    'Generate file disbursement
                    Dim strQuery As String = "EXEC " & strSPGenerate & " " & objTrxDafnom_OTP.Pk_TrxDafnom_OTP_Id.ToString
                    Dim result As New DataTable
                    result = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, strQuery)

                    If result.Rows.Count > 0 Then
                        If result.Rows(0).Item(0).ToString.Length > 0 Then
                            Dim strFileName As String = ""
                            strQuery = "EXEC " & strSPFilename & " '" & Date.Now.ToString("yyyyMMdd") & "'"
                            Dim dtFileName As Data.DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, strQuery)
                            If dtFileName.Rows.Count > 0 Then
                                strFileName = dtFileName.Rows(0).Item(0).ToString
                            End If

                            Dim ObjTrxDafnom As TrxDafnom = ObjDB.TrxDafnoms.Where(Function(x) x.Pk_TrxDafnom_Id = objTrxDafnom_OTP.Fk_TrxDafnom_Id).FirstOrDefault
                            If Not ObjTrxDafnom Is Nothing Then
                                Dim ObjTrxLimit As TrxLimit = ObjDB.TrxLimits.Where(Function(x) x.Pk_TrxLimit_Id = ObjTrxDafnom.Fk_TrxLimit_Id).FirstOrDefault
                                If Not ObjTrxLimit Is Nothing Then

                                    Dim strFilePath As String = strLocalFolderPath & "\" & strFileName
                                    System.IO.File.WriteAllText(strFilePath, result.Rows(0).Item(0).ToString)

                                    Dim ObjNCBSFile As New TrxOTP_NCBSFile
                                    With ObjNCBSFile
                                        .Fk_TrxDafnom_OTP_Id = objTrxDafnom_OTP.Pk_TrxDafnom_OTP_Id
                                        .Fk_MsStatusPengiriman_Id = 6
                                        .NCBSFileName = strFileName
                                        .NCBSFileContent = System.IO.File.ReadAllBytes(strFilePath)
                                        .Fk_MsNCBSFileType_Id = ObjFk_MsNCBSFileType_Id
                                        .GeneratedDate = Date.Now
                                    End With
                                    ObjDB.Entry(ObjNCBSFile).State = Entity.EntityState.Added
                                    ObjDB.SaveChanges()
                                    lngReturn = ObjNCBSFile.Pk_TrxOTP_NCBSFile_Id

                                    'Update Status file Disbursement
                                    Dim ObjTrxOTP_NCBSFileDisburse As TrxOTP_NCBSFile = ObjDB.TrxOTP_NCBSFile.Where(Function(x) x.Pk_TrxOTP_NCBSFile_Id = lngPk_TrxOTP_NCBSFile_Id).FirstOrDefault
                                    If Not ObjTrxOTP_NCBSFileDisburse Is Nothing Then
                                        If Not ObjTrxOTP_NCBSFileDisburse.Fk_MsStatusPengiriman_Id = 13 Then
                                            With ObjTrxOTP_NCBSFileDisburse
                                                .Fk_MsStatusPengiriman_Id = 5
                                            End With
                                            ObjDB.Entry(ObjTrxOTP_NCBSFileDisburse).State = Entity.EntityState.Modified
                                            ObjDB.SaveChanges()
                                        End If

                                    End If

                                End If
                            End If
                        End If
                    End If
                End If

                Return lngReturn
            Catch ex As Exception

            End Try
        End Using
    End Function

    'update issue production kunto
    Private Function ReadNoUrut(ByVal lngFk_NCBSFile_Id As Long) As Long
        Dim lngReturn As Long = 0
        Dim strLocalFolderPath As String = GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath)
        Using ObjDB As New NawaDataEntities
            Try
                Dim objNCBSFile As TrxOTP_NCBSFile = ObjDB.TrxOTP_NCBSFile.Where(Function(x) x.Pk_TrxOTP_NCBSFile_Id = lngFk_NCBSFile_Id).FirstOrDefault
                Dim strFilePath As String = strLocalFolderPath & "\" & objNCBSFile.NCBSFileName

                For Each strLine As String In System.IO.File.ReadLines(strFilePath)
                    If strLine.Length > 0 Then
                        Select Case Left(strLine, 1)
                            Case "1"
                                Dim no_urutNCBS As Integer = strLine.Split("|")(16)
                                lngReturn = no_urutNCBS
                        End Select
                    End If
                Next

                Return lngReturn
            Catch ex As Exception

            End Try
        End Using
    End Function



    Private Function GenerateFilePayoff(ByVal lngFk_TrxOTP_Pelunasan_Id As Long, ByVal strSPGenerate As String, ByVal strSPFilename As String, ByVal ObjFk_MsNCBSFileType_Id As EnumMsNCBSFileType) As Long
        Dim lngReturn As Long = 0
        Dim strLocalFolderPath As String = GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath)
        Using ObjDB As New NawaDataEntities
            Try
                Dim objTrxOTP_Pelunasan As TrxOTP_Pelunasan = ObjDB.TrxOTP_Pelunasan.Where(Function(x) x.Pk_TrxOTP_Pelunasan_Id = lngFk_TrxOTP_Pelunasan_Id).FirstOrDefault
                If Not objTrxOTP_Pelunasan Is Nothing Then
                   
                    mylog.LogInfo("PK pelunasan " & objTrxOTP_Pelunasan.Pk_TrxOTP_Pelunasan_Id.ToString)
                    'Generate file disbursement
                    Dim strQuery As String = "EXEC " & strSPGenerate & " " & objTrxOTP_Pelunasan.Pk_TrxOTP_Pelunasan_Id.ToString
                    Dim result As New DataTable
                    result = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, strQuery)
                    If result.Rows.Count > 0 Then

                        mylog.LogInfo(" ROws Count > 0 & 1 & PK " & objTrxOTP_Pelunasan.Pk_TrxOTP_Pelunasan_Id.ToString)
                        If result.Rows(0).Item(0).ToString.Length > 0 Then

                            mylog.LogInfo(" ROws Count > 0 & 2")
                            Dim strFileName As String = ""
                            strQuery = "EXEC " & strSPFilename & " '" & Date.Now.ToString("yyyyMMdd") & "'"
                            Dim dtFileName As Data.DataTable = SQLHelper.ExecuteTable(SQLHelper.strConnectionString, CommandType.Text, strQuery)
                            If dtFileName.Rows.Count > 0 Then
                                strFileName = dtFileName.Rows(0).Item(0).ToString
                            End If

                            Dim ObjTrxDafnom_OTP As TrxDafnom_OTP = ObjDB.TrxDafnom_OTP.Where(Function(x) x.Pk_TrxDafnom_OTP_Id = objTrxOTP_Pelunasan.Fk_TrxDafnom_OTP_Id).FirstOrDefault
                            If Not ObjTrxDafnom_OTP Is Nothing Then

                                Dim ObjTrxDafnom As TrxDafnom = ObjDB.TrxDafnoms.Where(Function(x) x.Pk_TrxDafnom_Id = ObjTrxDafnom_OTP.Fk_TrxDafnom_Id).FirstOrDefault
                                If Not ObjTrxDafnom Is Nothing Then

                                    Dim ObjTrxLimit As TrxLimit = ObjDB.TrxLimits.Where(Function(x) x.Pk_TrxLimit_Id = ObjTrxDafnom.Fk_TrxLimit_Id).FirstOrDefault
                                    If Not ObjTrxLimit Is Nothing Then

                                        Dim strFilePath As String = strLocalFolderPath & "\" & strFileName
                                        System.IO.File.WriteAllText(strFilePath, result.Rows(0).Item(0).ToString)


                                        ' Add No_urut
                                        'For Each strLine As String In System.IO.File.ReadLines(strFilePath)
                                        '    If strLine.Length > 0 Then
                                        '        Select Case Left(strLine, 1)
                                        '            Case "1"
                                        '                Dim no_urutNCBS As Integer = strLine.Split("|")(16)
                                        '                Dim objTrxPelunasan As TrxOTP_Pelunasan = ObjDB.TrxOTP_Pelunasan.Where(Function(x) x.Pk_TrxOTP_Pelunasan_Id = lngFk_TrxOTP_Pelunasan_Id).FirstOrDefault
                                        '                If Not objTrxPelunasan Is Nothing Then
                                        '                    mylog.LogInfo("No Urut before" & objTrxPelunasan.No_Urut.ToString)
                                        '                    objTrxPelunasan.No_Urut = no_urutNCBS
                                        '                    mylog.LogInfo("No Urut after" & objTrxPelunasan.No_Urut.ToString)

                                        '                    'Save
                                        '                    ObjDB.Entry(objTrxPelunasan).State = Entity.EntityState.Modified
                                        '                    ObjDB.SaveChanges()
                                        '                End If
                                        '        End Select
                                        '    End If
                                        'Next




                                        Dim ObjNCBSFile As New TrxOTP_NCBSFile
                                        With ObjNCBSFile
                                            .Fk_TrxDafnom_OTP_Id = objTrxOTP_Pelunasan.Fk_TrxDafnom_OTP_Id
                                            .Fk_MsStatusPengiriman_Id = 10
                                            .NCBSFileName = strFileName
                                            .NCBSFileContent = System.IO.File.ReadAllBytes(strFilePath)
                                            If objTrxOTP_Pelunasan.Fk_MsJenisOTPPelunasan_Id.GetValueOrDefault(0) = 2 Then

                                                ObjFk_MsNCBSFileType_Id = 6
                                            End If
                                            .Fk_MsNCBSFileType_Id = ObjFk_MsNCBSFileType_Id
                                            .GeneratedDate = Date.Now
                                        End With
                                        ObjDB.Entry(ObjNCBSFile).State = Entity.EntityState.Added
                                        ObjDB.SaveChanges()
                                        lngReturn = ObjNCBSFile.Pk_TrxOTP_NCBSFile_Id
                                    End If
                                End If
                            End If
                        End If
                    End If

                End If

                Return lngReturn
            Catch ex As Exception
                mylog.LogError("An Error has been occurred on Generate Payoff ", ex)
            End Try
        End Using
    End Function

    Public Function GetSFTPSettingValue(ByVal ObjEnumSFTPSetting As EnumSFTPSetting) As String
        Dim StrReturn As String = ""
        Using ObjDB As New NawaDataEntities
            Try
                Dim ObjSystemParameter As SystemParameter = ObjDB.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = CInt(ObjEnumSFTPSetting)).FirstOrDefault

                If Not ObjSystemParameter Is Nothing Then
                    If ObjSystemParameter.IsEncript Then
                        StrReturn = Common.DecryptRijndael(ObjSystemParameter.SettingValue, ObjSystemParameter.EncriptionKey)
                    Else
                        StrReturn = ObjSystemParameter.SettingValue
                    End If
                End If

                'Dim StrQuery As String = "SELECT sp.SettingValue FROM SystemParameter sp WHERE PK_SystemParameter_ID = " & CInt(ObjEnumSFTPSetting)
                'StrReturn = CStr(SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, StrQuery, Nothing))

            Catch ex As Exception

            End Try
        End Using

        Return StrReturn
    End Function
    'Public Function GetSFTPSettingValue(ByVal ObjEnumSFTPSetting As EnumSFTPSetting) As String
    '    Dim StrReturn As String = ""
    '    Try
    '        Dim StrQuery As String = "SELECT sp.SettingValue FROM SystemParameter sp WHERE PK_SystemParameter_ID = " & CInt(ObjEnumSFTPSetting)
    '        StrReturn = CStr(SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, StrQuery, Nothing))
    '    Catch ex As Exception

    '    End Try

    '    Return StrReturn
    'End Function

    Enum EnumMsNCBSFileType
        Disbursement = 1
        GEFUBDI = 2
        GEFUNonBDI = 3
        HoldFund = 4
        UnholdFund = 5
        FullPayoff = 6
        PartialPayoff = 7
    End Enum

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class

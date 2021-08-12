Option Explicit On
Option Strict On

Imports WinSCP

Public Class SFTPBLL
    Implements IDisposable

    Public Function IsSFTPFileExist(ByVal strFileName As String, ByVal SFTPFolderPath As EnumSFTPSetting) As Boolean
        Dim BolReturn As Boolean = False
        Dim mySessionOptions As New SessionOptions
        With mySessionOptions
            .Protocol = Protocol.Sftp
            .HostName = GetSFTPSettingValue(EnumSFTPSetting.HostName)
            .UserName = GetSFTPSettingValue(EnumSFTPSetting.UserName)
            .Password = GetSFTPSettingValue(EnumSFTPSetting.Password)
            .PortNumber = CInt(GetSFTPSettingValue(EnumSFTPSetting.PortNumber))
            .SshHostKeyFingerprint = GetSFTPSettingValue(EnumSFTPSetting.SshHostKeyFingerprint)
        End With
        Using mySession As Session = New Session
            ' Connect
            mySession.Open(mySessionOptions)

            Dim strFilePath As String = ""
            strFilePath = GetSFTPSettingValue(SFTPFolderPath) & strFileName

            If mySession.FileExists(strFilePath) Then
                BolReturn = True
            End If
        End Using

        Return BolReturn
    End Function

    Public Function PutFileSFTPPSFTP(ByVal SFTPFolderPath As EnumSFTPSetting) As Boolean

    End Function

    Public Function PutFileSFTP(ByVal SFTPFolderPath As EnumSFTPSetting) As Boolean
        If GetSFTPSettingValue(EnumSFTPSetting.CekSFTP) = "1" Then
            Dim mySessionOptions As New SessionOptions
            With mySessionOptions
                .Protocol = Protocol.Sftp
                .HostName = GetSFTPSettingValue(EnumSFTPSetting.HostName)
                .UserName = GetSFTPSettingValue(EnumSFTPSetting.UserName)
                .Password = GetSFTPSettingValue(EnumSFTPSetting.Password)
                .PortNumber = CInt(GetSFTPSettingValue(EnumSFTPSetting.PortNumber))
                .SshHostKeyFingerprint = GetSFTPSettingValue(EnumSFTPSetting.SshHostKeyFingerprint)
            End With

            Using mySession As Session = New Session
                ' Connect
                mySession.Open(mySessionOptions)

                ' Upload files
                Dim myTransferOptions As New TransferOptions
                myTransferOptions.TransferMode = TransferMode.Binary

                Dim transferResult As TransferOperationResult
                transferResult = mySession.PutFiles(GetSFTPSettingValue(EnumSFTPSetting.LocalFolderPath) & "\*", GetSFTPSettingValue(SFTPFolderPath), True, myTransferOptions)

                ' Throw on any error
                transferResult.Check()
            End Using
        End If
    End Function

    'Public Function GetFileSFTP(ByVal ProcessDate As DateTime) As Boolean
    '    If GetSFTPSettingValue(EnumSFTPSetting.CekSFTP) = "1" Then
    '        Dim mySessionOptions As New SessionOptions
    '        With mySessionOptions
    '            .Protocol = Protocol.Sftp
    '            .HostName = GetSFTPSettingValue(EnumSFTPSetting.HostName)
    '            .UserName = GetSFTPSettingValue(EnumSFTPSetting.UserName)
    '            .PortNumber = CInt(GetSFTPSettingValue(EnumSFTPSetting.PortNumber))
    '            .SshHostKeyFingerprint = GetSFTPSettingValue(EnumSFTPSetting.SshHostKeyFingerprint)

    '            'Utk password ketika disimpan di table dalam kondisi diencrypt
    '            Dim strPassword As String = GetSFTPSettingValue(EnumSFTPSetting.Password)
    '            Dim sahassakey As String = "S@h@ss@"
    '            .Password = DecryptRijndael(strPassword, sahassakey)
    '        End With

    '        Using mySession As Session = New Session
    '            ' Connect
    '            mySession.Open(mySessionOptions)

    '            ' Upload files
    '            Dim myTransferOptions As New TransferOptions
    '            myTransferOptions.TransferMode = TransferMode.Binary

    '            'Dim strView As String = "SELECT lqv.FileNameFormat FROM ListQueryView AS lqv"
    '            'Dim StrFileName As String = Common.getStringFieldValue(strView)
    '            'StrFileName = My.Settings.ExportImportTexfilePath & "\" & Replace(StrFileName, "@ProcessDate", ProcessDate.ToString("yyyyMMdd")) & ".txt"

    '            Dim transferResult As TransferOperationResult
    '            transferResult = mySession.GetFiles(GetSFTPSettingValue(EnumSFTPSetting.PathFolder) & "/*", My.Settings.ExportImportTexfilePath & "\*", True, myTransferOptions)

    '            ' Throw on any error
    '            transferResult.Check()
    '        End Using
    '    End If
    'End Function

    'Public Shared Function DecryptRijndael(ByVal cleanString As String, ByVal cleansalt As String) As String
    '    Dim sym As New Encryption.Symmetric(Encryption.Symmetric.Provider.Rijndael)
    '    Dim key As New Encryption.Data(cleansalt)
    '    Dim encryptedData As New Encryption.Data
    '    encryptedData.Base64 = cleanString

    '    Dim decryptedData As Encryption.Data
    '    decryptedData = sym.Decrypt(encryptedData, key)
    '    Return decryptedData.ToString
    'End Function
    'Public Shared Function EncryptRijndael(ByVal cleanString As String, ByVal cleansalt As String) As String
    '    Dim sym As New Encryption.Symmetric(Encryption.Symmetric.Provider.Rijndael)
    '    Dim key As New Encryption.Data(cleansalt)
    '    Dim encryptedData As Encryption.Data
    '    encryptedData = sym.Encrypt(New Encryption.Data(cleanString), key)

    '    Dim base64EncryptedString As String = encryptedData.ToBase64
    '    Return base64EncryptedString
    'End Function

    Public Function GetSFTPSettingValue(ByVal ObjEnumSFTPSetting As EnumSFTPSetting) As String
        Dim StrReturn As String = ""
        Try
            Dim StrQuery As String = "SELECT sp.SettingValue FROM SystemParameter sp WHERE PK_SystemParameter_ID = " & CInt(ObjEnumSFTPSetting)
            StrReturn = CStr(SQLHelper.ExecuteScalar(SQLHelper.strConnectionString, CommandType.Text, StrQuery, Nothing))
        Catch ex As Exception

        End Try

        Return StrReturn
    End Function


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

Public Enum EnumSFTPSetting
    HostName = 5001
    UserName = 5002
    Password = 5003
    SshHostKeyFingerprint = 5005
    PortNumber = 5004
    PathFolderFT = 5006
    PathFolderDC = 5007
    CekSFTP = 5000
    LocalFolderPath = 5008
    sessionname = 6000
End Enum

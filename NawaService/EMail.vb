Imports System.Configuration
Imports System.Net.Mail

Public Class EMail
    Private m_sSender As String
    Private m_sRecipient As String
    Private m_sRecipientCC As String
    Private m_sRecipientBCC As String
    Private m_sServer As String
    Private m_sSubject As String
    Private m_sBody As String
    Private m_mPriority As MailPriority
    Private m_sMailAttachment As String
    Private m_sLastErrorMessage As String
    Private m_IsBodyHtml As Boolean
    Private m_bLastSendState As Boolean
    Private mylog As New NawaConsoleLog
    'Private Shared ReadOnly myLog As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    ''' <summary>
    ''' Get or set Subject of email
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' [Johan Peterson] 2007-04-30	Created
    ''' </history>
    Public Property Subject() As String
        Get
            Subject = m_sSubject
        End Get
        Set(ByVal s As String)
            m_sSubject = s
        End Set
    End Property

    ''' <summary>
    ''' Get or set Body of email
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Body() As String
        Get
            Body = m_sBody
        End Get
        Set(ByVal s As String)
            m_sBody = s
        End Set
    End Property

    ''' <summary>
    ''' Get or set Sender of email
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Sender() As String
        Get
            Sender = m_sSender
        End Get
        Set(ByVal s As String)
            m_sSender = s
        End Set
    End Property

    ''' <summary>
    ''' Get or set Recipient of email. For multiple recipient, separate it using ";"
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Recipient() As String
        Get
            Recipient = m_sRecipient
        End Get
        Set(ByVal s As String)
            m_sRecipient = s
        End Set
    End Property

    ''' <summary>
    ''' Get or set Recipient CC of email. For multiple recipient, separate it using ";"
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property RecipientCC() As String
        Get
            RecipientCC = m_sRecipientCC
        End Get
        Set(ByVal s As String)
            m_sRecipientCC = s
        End Set
    End Property

    ''' <summary>
    ''' Get or set Recipient BCC of email. For multiple recipient, separate it using ";"
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property RecipientBCC() As String
        Get
            RecipientBCC = m_sRecipientBCC
        End Get
        Set(ByVal s As String)
            m_sRecipientBCC = s
        End Set
    End Property

    ''' <summary>
    ''' Get or set value of SMTP Server
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Server() As String
        Get
            Server = m_sServer
        End Get
        Set(ByVal s As String)
            m_sServer = s
        End Set
    End Property

    ''' <summary>
    ''' Get or set priority of the email
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Priority() As MailPriority
        Get
            Priority = m_mPriority
        End Get
        Set(ByVal p As MailPriority)
            m_mPriority = p
        End Set
    End Property

    ''' <summary>
    ''' Get or set mail attachment with fullpath of filename. For multiple attachment, separate it using ";"
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MailAttachment() As String
        Get
            MailAttachment = m_sMailAttachment
        End Get
        Set(ByVal Value As String)
            m_sMailAttachment = Value
        End Set
    End Property

    ''' <summary>
    ''' Get or set last send state.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LastSendState() As Boolean
        Get
            LastSendState = m_bLastSendState
        End Get
        Set(ByVal Value As Boolean)
            m_bLastSendState = Value
        End Set
    End Property

    ''' <summary>
    ''' Get or set mail body type is HTML or not
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsBodyHtml() As Boolean
        Get
            Return m_IsBodyHtml
        End Get
        Set(ByVal value As Boolean)
            m_IsBodyHtml = value
        End Set
    End Property

    ''' <summary>
    ''' Get last error message
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property LastErrorMessage() As String
        Get
            Return m_sLastErrorMessage
        End Get
    End Property

    ''' <summary>
    ''' Initial email
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Init()
        m_sBody = ""
        m_sSubject = ""
        m_sSender = ""
        m_sRecipient = ""
        m_sRecipientCC = ""
        m_sRecipientBCC = ""
        m_sLastErrorMessage = ""

        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim objsysparam As SystemParameter = objDb.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = 2000).FirstOrDefault
            If Not objsysparam Is Nothing Then
                m_sServer = objsysparam.SettingValue
            End If
        End Using


        m_mPriority = MailPriority.Normal

        Using objDb As NawaDataEntities = New NawaDataEntities
            Dim objsysparam As SystemParameter = objDb.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = 2011).FirstOrDefault
            If Not objsysparam Is Nothing Then
                m_IsBodyHtml = objsysparam.SettingValue
            End If
        End Using


        m_bLastSendState = False
    End Sub

    ''' <summary>
    ''' New implementation of Email class
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        Init()
    End Sub

    ''' <summary>
    ''' Dispose implementation of Email class
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Dispose()

    End Sub

    ''' <summary>
    ''' To initial email with default initial value
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Clear()
        Init()
    End Sub

    ''' <summary>
    ''' Check if recipient and sender is valid
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function isValidMail() As Boolean
        If m_sSender.Trim.Length > 0 Then
            If m_sRecipient.Trim.Length > 0 Then
                isValidMail = True
            Else
                isValidMail = False
                m_sLastErrorMessage = "Invalid Recipient Address"
            End If
        Else
            isValidMail = False
            m_sLastErrorMessage = "Invalid Sender Address"
        End If
    End Function

    ''' <summary>
    ''' 'Send Email Synchronously with all properties.
    ''' </summary>
    ''' <returns>Return TRUE if send mail done successfully, otherwise return FALSE</returns>
    ''' <remarks></remarks>
    Public Function SendEmail() As Boolean
        'For a Windows Forms Application, add a reference to 
        'System.Web.dll using [Project Add References]


        Dim email As MailMessage
        Dim chrDelimiter As Char = ";"c
        Dim arrFileName() As String
        Dim intFileNameCounter As Integer
        Dim att As Attachment
        Dim blnSend As Boolean
        Dim smtpMail As SmtpClient
        Dim Counter As Integer
        Dim arrMailAddress() As String
        Try
            smtpMail = New SmtpClient(m_sServer)
            If isValidMail() Then
                smtpMail.DeliveryMethod = SmtpDeliveryMethod.Network

                email = New MailMessage
                'The email address of the sender
                email.From = New MailAddress(m_sSender)

                'The email address of the recipient. 
                'Seperate multiple email adresses with a comma seperator
                m_sRecipient = m_sRecipient.Replace(",", ";")
                arrMailAddress = m_sRecipient.Split(chrDelimiter)
                For Counter = 0 To arrMailAddress.Length - 1
                    email.To.Add(arrMailAddress(Counter))
                Next

                If m_sRecipientCC.Trim.Length > 0 Then
                    m_sRecipientCC = m_sRecipientCC.Replace(",", ";")
                    arrMailAddress = m_sRecipientCC.Split(chrDelimiter)
                    For Counter = 0 To arrMailAddress.Length - 1
                        email.CC.Add(arrMailAddress(Counter))
                    Next
                End If

                If m_sRecipientBCC.Trim.Length > 0 Then
                    m_sRecipientBCC = m_sRecipientBCC.Replace(",", ";")
                    arrMailAddress = m_sRecipientBCC.Split(chrDelimiter)
                    For Counter = 0 To arrMailAddress.Length - 1
                        email.Bcc.Add(arrMailAddress(Counter))
                    Next
                End If

                'The subject of the email
                email.Subject = m_sSubject

                'The Priority attached and displayed for the email
                email.Priority = m_mPriority

                'The format of the contents of the email
                'The email format can either be MailFormat.Html or MailFormat.Text
                'MailFormat.Html : Html Content
                'MailFormat.Text : Text Message
                email.IsBodyHtml = m_IsBodyHtml

                'The contents of the email 
                email.Body = m_sBody

                'EMail Attachment 
                ' Build an IList of mail attachments using the files named in the string.
                If Not m_sMailAttachment Is Nothing Then
                    If m_sMailAttachment.Trim.Length > 0 Then
                        arrFileName = m_sMailAttachment.Split(chrDelimiter)
                        For intFileNameCounter = 0 To arrFileName.Length - 1
                            att = New Attachment(arrFileName(intFileNameCounter).ToString)
                            email.Attachments.Add(att)
                            att = Nothing
                        Next
                    End If
                End If

                'The name of the smtp server to use for sending emails
                'SmtpMail.SmtpServer is commonly ignored in many applications 


                'Send the email and handle any error that occurs
                Try
                    Dim strpassword As String

                    Using objDb As NawaDataEntities = New NawaDataEntities
                        Dim objsysparam As SystemParameter = objDb.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = 2003).FirstOrDefault
                        If Not objsysparam Is Nothing Then
                            strpassword = objsysparam.SettingValue
                        End If
                    End Using

                    Dim struserEmail As String
                    Using objDb As NawaDataEntities = New NawaDataEntities
                        Dim objsysparam As SystemParameter = objDb.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = 2002).FirstOrDefault
                        If Not objsysparam Is Nothing Then
                            struserEmail = objsysparam.SettingValue
                        End If
                    End Using

                    Dim strport As String = ""
                    Using objDb As NawaDataEntities = New NawaDataEntities
                        Dim objsysparam As SystemParameter = objDb.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = 2001).FirstOrDefault
                        If Not objsysparam Is Nothing Then
                            strport = objsysparam.SettingValue
                        End If
                    End Using

                    Dim bssl As Boolean = False
                    Using objDb As NawaDataEntities = New NawaDataEntities
                        Dim objsysparam As SystemParameter = objDb.SystemParameters.Where(Function(x) x.PK_SystemParameter_ID = 2004).FirstOrDefault
                        If Not objsysparam Is Nothing Then
                            bssl = objsysparam.SettingValue
                        End If
                    End Using

                    smtpMail.EnableSsl = bssl
                    smtpMail.Port = strport
                    smtpMail.Credentials = New Net.NetworkCredential(struserEmail, strpassword)
                    smtpMail.Send(email)
                    blnSend = True
                Catch ex As Exception
                    mylog.LogError("Failed send email to " & m_sRecipient & " with subject " & m_sSubject & " cause: " & ex.Message, ex)

                    m_sLastErrorMessage = ex.Message
                    blnSend = False
                    Throw
                End Try
            Else
                blnSend = False
            End If
            m_bLastSendState = blnSend
            Return blnSend
        Catch ex As Exception
            m_sLastErrorMessage = ex.Message
            blnSend = False
            Throw ex
        Finally
            smtpMail = Nothing
        End Try
    End Function

End Class

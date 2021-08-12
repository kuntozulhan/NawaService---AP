Public Interface IProcessEvent
    ''' <summary>
    ''' Event yang terjadi pada saat error, akan menampilkan error code dan message
    ''' </summary>
    ''' <param name="EventSource"></param>
    ''' <param name="ErrorCode"></param>
    ''' <param name="Source"></param>
    ''' <param name="Description"></param>
    ''' <remarks></remarks>
    Event OnError(ByVal EventSource As String, ByVal ErrorCode As Integer, ByVal Source As String, ByVal Description As String)

    ''' <summary>
    ''' Event yang menyebabkan progress status jadi finished atau canceled dengan melihat flag Canceled
    ''' </summary>
    ''' <param name="EventSource"></param>
    ''' <param name="Canceled"></param>
    ''' <remarks></remarks>
    Event OnFinish(ByVal EventSource As String, ByVal Canceled As Boolean)

    ''' <summary>
    ''' Event yang memberikan keterangan mengenai jalannya proses.
    ''' </summary>
    ''' <param name="EventSource"></param>
    ''' <param name="ProgressCount"></param>
    ''' <param name="Description"></param>
    ''' <remarks></remarks>
    Event OnProgress(ByVal EventSource As String, ByVal ProgressCount As Integer, ByVal Description As String, ByVal ValueType As EnumProgressValueType)

    ''' <summary>
    ''' Event yang menunggu flag Cancel, apabila flag Cancel di set (True) maka proses akan di batalkan.
    ''' </summary>
    ''' <param name="EventSource"></param>
    ''' <param name="pbCancel"></param>
    ''' <remarks></remarks>
    Event OnQueryCancel(ByVal EventSource As String, ByRef pbCancel As Boolean)

    ''' <summary>
    ''' Event yang menyebabkan progress status jadi running, dengan jumlah total data yang akan di proses bila ada
    ''' </summary>
    ''' <param name="EventSource"></param>
    ''' <param name="TotalCount"></param>
    ''' <remarks></remarks>
    Event OnStart(ByVal EventSource As String, ByVal TotalCount As Integer)

    ''' <summary>
    ''' Event yang menyebabkan progress status jadi waiting
    ''' </summary>
    ''' <param name="EventSource"></param>
    ''' <remarks></remarks>
    Event OnInitial(ByVal EventSource As String)

    ''' <summary>
    ''' Event yang pertama kali terjadi saat suatu proses terjadi
    ''' </summary>
    ''' <param name="EventSource"></param>
    ''' <remarks></remarks>
    Event OnThreadStart(ByVal EventSource As String)

    ''' <summary>
    ''' Event yang terakhir kali terjadi saat suatu proses selesai, baik dengan result error maupun success.
    ''' </summary>
    ''' <param name="EventSource"></param>
    ''' <remarks></remarks>
    Event OnThreadFinish(ByVal EventSource As String)

End Interface

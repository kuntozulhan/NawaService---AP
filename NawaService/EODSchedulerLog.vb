'------------------------------------------------------------------------------
' <auto-generated>
'    This code was generated from a template.
'
'    Manual changes to this file may cause unexpected behavior in your application.
'    Manual changes to this file will be overwritten if the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic

Partial Public Class EODSchedulerLog
    Public Property PK_EODSchedulerLog_ID As Long
    Public Property FK_EODSchedulerID As Nullable(Of Long)
    Public Property ExecuteBy As String
    Public Property ProcessDate As Nullable(Of Date)
    Public Property StartDate As Nullable(Of Date)
    Public Property Enddate As Nullable(Of Date)
    Public Property ErrorMessage As String
    Public Property FK_MsEODStatus_ID As Nullable(Of Integer)
    Public Property DataDate As Nullable(Of Date)

End Class

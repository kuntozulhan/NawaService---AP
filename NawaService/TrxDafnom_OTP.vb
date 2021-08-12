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

Partial Public Class TrxDafnom_OTP
    Public Property Pk_TrxDafnom_OTP_Id As Long
    Public Property Fk_TrxDafnom_Id As Nullable(Of Long)
    Public Property Tenor As Nullable(Of Integer)
    Public Property MsAccountOfficerCode As String
    Public Property MsMISCode As String
    Public Property MsProductCode As String
    Public Property Fk_MsJenisOTP_Id As Nullable(Of Integer)
    Public Property NoPromes As String
    Public Property TanggalPencairan As Nullable(Of Date)
    Public Property PeriodStart As Nullable(Of Date)
    Public Property PeriodEnd As Nullable(Of Date)
    Public Property Nominal As Nullable(Of Decimal)
    Public Property MarginEquivalent As Nullable(Of Decimal)
    Public Property NisbahBagiHasilBank As Nullable(Of Decimal)
    Public Property NisbahBagiHasilNasabah As Nullable(Of Decimal)
    Public Property Fk_MsKondisiMargin_Id As Nullable(Of Integer)
    Public Property TanggalDebetAngsuran As Nullable(Of Integer)
    Public Property Fk_MsJenisPencairan_Id As Nullable(Of Integer)
    Public Property BiayaRTGS As Nullable(Of Decimal)
    Public Property Fk_MsStatusOTP_Id As Nullable(Of Integer)
    Public Property Fk_MsStatusDokumen_Id As Nullable(Of Integer)
    Public Property CreatedBy As String
    Public Property LastUpdateBy As String
    Public Property ApprovedBy As String
    Public Property CreatedDate As Nullable(Of Date)
    Public Property LastUpdateDate As Nullable(Of Date)
    Public Property ApprovedDate As Nullable(Of Date)
    Public Property ApprovalReason As String
    Public Property LTSApprovalReason As String
    Public Property AccountNo As String
    Public Property NomorRekeningCASA As String
    Public Property MsScheduleCode As String

    Public Overridable Property TrxDafnom As TrxDafnom
    Public Overridable Property TrxDafnom_Detail As ICollection(Of TrxDafnom_Detail) = New HashSet(Of TrxDafnom_Detail)

End Class

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

Partial Public Class TrxDafnom_Detail
    Public Property Pk_TrxDafnom_Detail_Id As Long
    Public Property Fk_TrxDafnom_Id As Long
    Public Property Fk_TrxDafnom_OTP_Id As Nullable(Of Long)
    Public Property NamaKaryawan As String
    Public Property NoKTP As String
    Public Property NIK As String
    Public Property TanggalLahir As Nullable(Of Date)
    Public Property NoTelepon As String
    Public Property TanggalPengangkatan As Nullable(Of Date)
    Public Property MasaKerja As Nullable(Of Integer)
    Public Property Fk_MsTujuanPembiayaan_Id As Nullable(Of Integer)
    Public Property Plafond As Nullable(Of Decimal)
    Public Property Tenor As Nullable(Of Integer)
    Public Property Angsuran As Nullable(Of Decimal)
    Public Property TakeHomePay As Nullable(Of Decimal)
    Public Property AngsuranKoperasiYgDimiliki As Nullable(Of Decimal)
    Public Property OutstandingTerakhir As Nullable(Of Decimal)
    Public Property Fk_MsSumberDana_Id As Nullable(Of Integer)
    Public Property SisaJangkaWaktu As Nullable(Of Integer)
    Public Property IIR As Nullable(Of Decimal)
    Public Property RekeningAtasNama As String
    Public Property RekeningNamaBank As String
    Public Property RekeningNomor As String

    Public Overridable Property TrxDafnom As TrxDafnom
    Public Overridable Property TrxDafnom_OTP As TrxDafnom_OTP

End Class
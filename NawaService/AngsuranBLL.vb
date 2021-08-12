Imports System.Threading

Public Class AngsuranBLL
    Implements IDisposable

    Private mylog As New NawaConsoleLog

    Sub run()
        Try

            While True

                Thread.Sleep(My.Settings.IntThreadInterval)
                Using objDb As NawaDataEntities = New NawaDataEntities
                    objDb.Configuration.AutoDetectChangesEnabled = False
                    Dim lAng As List(Of Vw_MigrasiAngsuran) = objDb.Vw_MigrasiAngsuran.ToList
                    For Each ObjVw_MigrasiAngsuran As Vw_MigrasiAngsuran In lAng 'objDb.Vw_MigrasiAngsuran.ToList
                        Dim dblPlafond As Double = 0
                        Dim dblMargin As Double = ObjVw_MigrasiAngsuran.MarginEquivalent
                        Dim IntTenor As Integer = ObjVw_MigrasiAngsuran.Tenor
                        mylog.LogInfo("Tenor & Margin : " & IntTenor & " & " & dblMargin)
                        For Each ObjTrxDafnom_Detail As TrxDafnom_Detail In objDb.TrxDafnom_Detail.Where(Function(x) x.Fk_TrxDafnom_OTP_Id = ObjVw_MigrasiAngsuran.Pk_TrxDafnom_OTP_Id).ToList
                            dblPlafond = ObjTrxDafnom_Detail.Plafond

                            Dim objTrxDafnom_OTP As TrxDafnom_OTP = objDb.TrxDafnom_OTP.Where(Function(x) x.Pk_TrxDafnom_OTP_Id = ObjTrxDafnom_Detail.Fk_TrxDafnom_OTP_Id).FirstOrDefault
                            If Not objTrxDafnom_OTP Is Nothing Then
Temp:
                                objTrxDafnom_OTP.PeriodStart = objTrxDafnom_OTP.TanggalPencairan

                                Dim LastAngsuranDate As Date = objTrxDafnom_OTP.PeriodStart


                                Dim monthAdd As Date = New Date(Year(objTrxDafnom_OTP.TanggalPencairan), Month(objTrxDafnom_OTP.TanggalPencairan), objTrxDafnom_OTP.TanggalDebetAngsuran) 'Month(objTrxDafnom_OTP.TanggalPencairan) & "/" & objTrxDafnom_OTP.TanggalDebetAngsuran & "/" & Year(objTrxDafnom_OTP.TanggalPencairan)
                                Dim monthLess As Date = New Date(Year(objTrxDafnom_OTP.PeriodStart), Month(objTrxDafnom_OTP.PeriodStart), objTrxDafnom_OTP.TanggalDebetAngsuran) 'Month(objTrxDafnom_OTP.PeriodStart) & "/" & objTrxDafnom_OTP.TanggalDebetAngsuran & "/" & Year(objTrxDafnom_OTP.PeriodStart)
                                Dim tempDateAwal As Integer = 0


                                mylog.LogInfo("Before ShowAmortSched")
                                Dim ObjTrxOTP_AngsuranList As List(Of TrxOTP_Angsuran) = ShowAmortSched(dblPlafond, dblMargin, IntTenor)
                                mylog.LogInfo("After ShowAmortSched")
                                Dim ObjTrxOTP_AngsuranFirst As New TrxOTP_Angsuran
                                With ObjTrxOTP_AngsuranFirst
                                    .Fk_TrxDafnom_OTP_Id = ObjTrxDafnom_Detail.Fk_TrxDafnom_OTP_Id
                                    .Fk_TrxDafnom_Detail_Id = ObjTrxDafnom_Detail.Pk_TrxDafnom_Detail_Id

                                    If CInt(Day(objTrxDafnom_OTP.TanggalPencairan)) >= CInt(objTrxDafnom_OTP.TanggalDebetAngsuran) Then
                                        .TanggalAngsuran = objTrxDafnom_OTP.TanggalPencairan
                                        .AngsuranMargin = 0
                                    Else
                                        .TanggalAngsuran = monthAdd
                                        .AngsuranMargin = Math.Round(((DateDiff(DateInterval.Day, objTrxDafnom_OTP.TanggalPencairan.GetValueOrDefault(New Date(1900, 1, 1)), monthAdd) + 0) * dblPlafond / 360) * (dblMargin / 100.0), 2, MidpointRounding.AwayFromZero)

                                        'objTrxDafnom_OTP.TanggalPencairan.GetValueOrDefault(New Date(1900, 1, 1)), monthAdd) + 1) * dblPlafond / 360) * (dblMargin / 12 / 100.0) Update Kunto Issue other requirement 2020
                                    End If

                                    .AngsuranKe = 0
                                    .SisaPokok = dblPlafond
                                    .AngsuranPokok = 0
                                    .TotalAngsuran = 0

                                    objDb.Entry(ObjTrxOTP_AngsuranFirst).State = Data.Entity.EntityState.Added
                                End With

                                For Each ObjTrxOTP_AngsuranItem As TrxOTP_Angsuran In ObjTrxOTP_AngsuranList
                                    With ObjTrxOTP_AngsuranItem
                                        .Fk_TrxDafnom_OTP_Id = ObjTrxDafnom_Detail.Fk_TrxDafnom_OTP_Id
                                        .Fk_TrxDafnom_Detail_Id = ObjTrxDafnom_Detail.Pk_TrxDafnom_Detail_Id
                                        If CInt(Day(objTrxDafnom_OTP.TanggalPencairan)) = CInt(objTrxDafnom_OTP.TanggalDebetAngsuran) Then
                                            LastAngsuranDate = DateAdd(DateInterval.Month, 1, LastAngsuranDate)
                                            .TanggalAngsuran = LastAngsuranDate
                                        ElseIf CInt(Day(objTrxDafnom_OTP.TanggalPencairan)) > CInt(objTrxDafnom_OTP.TanggalDebetAngsuran) Then
                                            monthLess = DateAdd(DateInterval.Month, 1, monthLess)
                                            .TanggalAngsuran = monthLess
                                        Else
                                            tempDateAwal = Day(monthAdd)
                                            monthAdd = DateAdd(DateInterval.Month, 1, monthAdd)

                                            If CInt(objTrxDafnom_OTP.TanggalDebetAngsuran) <> tempDateAwal Then
                                                monthAdd = Month(monthAdd) & "/" & objTrxDafnom_OTP.TanggalDebetAngsuran & "/" & Year(monthAdd)
                                            End If
                                            tempDateAwal = Day(monthAdd)

                                            .TanggalAngsuran = monthAdd

                                        End If
                                        objDb.Entry(ObjTrxOTP_AngsuranItem).State = Data.Entity.EntityState.Added
                                    End With
                                Next
                                objDb.SaveChanges()

                            End If

                        Next

                    Next
                End Using

            End While
        Catch ex As Exception
            mylog.LogError("An Error has been occurred on Create Angsuran Migrasi: ", ex)
        End Try
    End Sub

    Private Function ShowAmortSched(decPrincipal As Decimal, decYearlyRate As Decimal, intNumMons As Integer) As List(Of TrxOTP_Angsuran)
        Dim ObjTrxOTP_Angsuran As New List(Of TrxOTP_Angsuran)

        'Test
        '  mylog.LogInfo("decPrincipal = " + decPrincipal.ToString() + " decYearlyRate = " + decYearlyRate.ToString() + " intNumMons = " + decYearlyRate.ToString())
        Dim strMsg As String, intStartMonth As Integer, decMonthlyRate As Decimal
        Dim decMonPayment As Decimal, decTotalInterest As Decimal
        Dim decYearInterest As Decimal, decOldBalance As Decimal
        Dim intMonthNum As Integer, decNewBalance As Decimal
        Dim decPrincipalPaid As Decimal, decInterestPaid As Decimal
        Dim decReductPrin As Decimal, intLoanYears As Integer
        'Display Amortization Schedule
        strMsg = "Please enter year (1-" & CStr(intNumMons / 12)
        strMsg = strMsg & ") for which amortization is to be shown:"
        intStartMonth = 1 '12 * Val(InputBox(strMsg)) - 11
        ''rtbDisplay.Text = "" 'clear RichTextbox of any previous output
        'change the attributes of the text that will be appended to the control with the next call to the AppendText method.
        'use the AppendText method if you want to change color of headers. See Page 320 on RichTextBox
        ''rtbDisplay.SelectionColor = Color.Blue
        ''rtbDisplay.AppendText(vbTab & vbTab & "Amount Paid " & vbTab & "Amount Paid " & vbTab & "Balance at" & vbCrLf)
        ''rtbDisplay.SelectionColor = Color.Blue
        ''rtbDisplay.AppendText(vbTab & "Month" & vbTab & "for Principal" & vbTab & "for Interest" & vbTab & "End of Month" & vbCrLf)
        decMonthlyRate = decYearlyRate / 12 / 100.0 'monthly interest rate
        mylog.LogInfo("Before MonthlyPayment")
        decMonPayment = MonthlyPayment(decPrincipal, decMonthlyRate, intNumMons)
        mylog.LogInfo("After MonthlyPayment")
        'Test
        ' mylog.LogInfo(decMonPayment.ToString())
        decTotalInterest = 0
        decYearInterest = 0
        decOldBalance = decPrincipal
        For intMonthNum = 0 To intNumMons - 1 'calculations done for all months here e.g. if 30 yr loan then intNumMons=360
            decNewBalance = Balance(decMonPayment, decOldBalance, decMonthlyRate)
            decPrincipalPaid = decOldBalance - decNewBalance
            decInterestPaid = decMonPayment - decPrincipalPaid 'rem: monthlyPayment = principal + interest
            decTotalInterest = decTotalInterest + decInterestPaid
            'if block will filter/show only those months for the year specified in the inputbox
            'If (intMonthNum >= intStartMonth) And (intMonthNum <= intStartMonth + 11) Then
            '    'rtbDisplay.AppendText(vbTab & FormatNumber(intMonthNum, 0)) ' Month number
            '    'rtbDisplay.AppendText(vbTab & FormatCurrency(decPrincipalPaid)) ' amount paid for principal
            '    'rtbDisplay.AppendText(vbTab & FormatCurrency(decInterestPaid)) ' amount paid for interest
            '    'rtbDisplay.AppendText(vbTab & FormatCurrency(decNewBalance) & vbCrLf) ' balance at end of month
            '    decYearInterest = decYearInterest + decInterestPaid
            'End If

            Dim ObjTrxOTP_AngsuranItem As New TrxOTP_Angsuran
            With ObjTrxOTP_AngsuranItem
                .AngsuranKe = intMonthNum + 1
                .SisaPokok = Math.Round(decNewBalance, 2, MidpointRounding.AwayFromZero)
                .AngsuranPokok = Math.Round(decPrincipalPaid, 2, MidpointRounding.AwayFromZero)
                .AngsuranMargin = Math.Round(decInterestPaid, 2, MidpointRounding.AwayFromZero)
                .TotalAngsuran = Math.Round(decMonPayment, 2, MidpointRounding.AwayFromZero)

                ObjTrxOTP_Angsuran.Add(ObjTrxOTP_AngsuranItem)
            End With

            decOldBalance = decNewBalance
        Next intMonthNum
        'rem: monthlyPayment = principal + interest
        decReductPrin = 12 * decMonPayment - decYearInterest
        intLoanYears = intNumMons / 12
        ''rtbDisplay.AppendText(vbCrLf) 'skip a line
        ''rtbDisplay.AppendText(vbTab & "Reduction in Principal:") 'i.e. for year specified in inputbox
        ''rtbDisplay.AppendText(vbTab & vbTab & vbTab & FormatCurrency(decReductPrin) & vbCrLf)
        ''rtbDisplay.AppendText(vbTab & "Interest Paid:") 'i.e. for year specified in inputbox
        ''rtbDisplay.AppendText(vbTab & vbTab & vbTab & FormatCurrency(decYearInterest) & vbCrLf)
        ''rtbDisplay.AppendText(vbTab & "Total Interest Over " & intLoanYears & " Years:")
        '''this sentence length crosses a number of tab positions
        ''rtbDisplay.AppendText(vbTab & vbTab & FormatCurrency(decTotalInterest))

        Return ObjTrxOTP_Angsuran
    End Function

    Private Function MonthlyPayment(decPrincipal As Decimal, decMonthlyRate As Decimal, intNumMons As Integer) As Decimal
        'Dim decPayEst As Decimal
        ''the standard formula for computing the monthly payment cannot be used if either
        ''the loan duration is zero months or the interest rate is zero percent.
        'If intNumMons = 0 Then
        '    decPayEst = decPrincipal
        'ElseIf decMonthlyRate = 0 Then
        '    decPayEst = decPrincipal / intNumMons
        'Else
        '    decPayEst = decPrincipal * decMonthlyRate / (1 - (1 + decMonthlyRate) ^ (-intNumMons))
        'End If
        'MonthlyPayment = Math.Round(decPayEst + 0.005, 2) 'round up to the nearest cent

        Dim decReturn As Decimal = 0
        Dim dblPayment As Double = 0
        Dim dblNumMons As Double = intNumMons

        'Eff In Adv
        'dblPayment = Pmt(decMonthlyRate, dblNumMons, decPrincipal,, 1)

        'Eff In Arr
        dblPayment = Pmt(decMonthlyRate, intNumMons, decPrincipal)

        If Not Double.IsNaN(dblPayment) Then
            decReturn = -dblPayment
        End If

        Return decReturn
    End Function

    Private Function Balance(decMonPayment As Decimal, decPrincipal As Decimal, decMonthlyRate As Decimal) As Decimal
        Dim decNewBal As Decimal 'Compute balance remaining to be paid at the end of the month
        decNewBal = (1 + decMonthlyRate) * decPrincipal
        If decNewBal <= decMonPayment Then 'e.g. the final monthly instalment to be paid
            decMonPayment = decNewBal
            Balance = 0
        Else
            Balance = decNewBal - decMonPayment
        End If
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

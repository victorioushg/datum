Imports MySql.Data.MySqlClient
Imports Microsoft.Win32
Public Class jsBanProChequeDevuelto
    Private Const sModulo As String = "Devoluci�n de cheques de clientes"
    Private Const nTabla As String = "tblcheque"

    Private strSQL As String

    Private myConn As New MySqlConnection
    Private ds As New DataSet
    Private dt As New DataTable
    Private CodigoCliente As String
    Private CodigoDivision As String
    Public Sub Cargar(ByVal MyCon As MySqlConnection)

        myConn = MyCon
        Me.Tag = sModulo

        IniciarCausasDevolucion()
        IniciarTxt()

        MensajeEtiqueta(lblInfo, "Seleccione o indique el cheque a devolver, escoja la fecha y la causa de devoluci�n ... ", TipoMensaje.iAyuda)
        lblLeyenda.Text = " Este proceso se aplica a un cheque que es devuelto por el banco donde fu� realizado el dep�sito de dicho " + vbCr + _
                " cheque. Genera una, dos o tres cuentas por cobrar al cliente, una basada en el monto del cheque devuelto, " + vbCr + _
                " otra por la comisi�n de devoluci�n y una �ltima por los gastos administrativos de la mencionada devoluci�n." + vbCr + _
                " Adem�s genera una Nota D�bito en el banco donde fu� depositado el cheque. "
        Me.Show()

    End Sub
    Private Sub IniciarTxt()

        txtCheque.Text = ""
        txtEstatus.Text = ""
        txtMonto.Text = ""
        txtBanco.Text = ""
        txtDeposito.Text = ""
        txtCliente.Text = ""
        txtAsesor.Text = ""
        txtFecha.Text = FormatoFecha(jytsistema.sFechadeTrabajo)
        txtCancelacion.Text = ""

        dg.Rows.Clear()

    End Sub
    Private Sub IniciarCausasDevolucion()

        Dim aCausa() As String = {"1. FECHA DEFECTUOSA ", "2. FECHA ADELANTADA", "3. FALTA FIRMA", _
            "4. DEFECTO FIRMA Y/O SELLO", "6. DEFECTO DE ENDOSO", "8. CUENTA CERRADA", _
            "9. GIRA SOBRE FONDOS NO DISPONIBLES", "10. PAGO SUSPENDIDO", "11. NO ES A NUESTRO CARGO", _
            "13. GIRADOR FALLECIDO", "14. PRESENTAR POR TAQUILLA", "15. DIRIGIRSE AL GIRADOR", _
            "16. CANTIDAD DEFECTUOSA", "17. FALTA SELLO COMPENSACION", "18. PASO 2 VECES COMPENSACION", _
            "19. CAUSA EXTERNA", "20. OTRA", ""}

        RellenaCombo(aCausa, cmbCausa)

    End Sub
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub jsBanProChequeDevuelto_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed

        dt.Dispose()
    End Sub

    Private Sub jsBanProChequeDevuelto_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Private Function Validado() As Boolean

        If txtCliente.Text = "" Then
            MensajeAdvertencia(lblInfo, " Este documento no tiene cliente v�lido...")
            Return False
        End If

        If txtCheque.Text = "" Then
            MensajeAdvertencia(lblInfo, " Debe indicar un n�mero de cheque v�lido...")
            Return False
        End If

        If txtEstatus.Text = "Devuelto" Then
            MensajeAdvertencia(lblInfo, "Este cheque YA est� devuelto...")
            Return False
        End If

        Return True

    End Function
    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click

        If Validado() Then
            GenerarProceso()
            ImprimirComprobante()
            MensajeInformativo(lblInfo, " DEVOLUCION DE CHEQUE REALIZADO CON EXITO ...")
        End If

        ProgressBar1.Value = 0
        lblProgreso.Text = ""

    End Sub

    Private Sub GenerarProceso()


        '1. Nota d�bito en el banco dep�sito 
        InsertEditBANCOSMovimientoBanco(myConn, lblInfo, True, CDate(txtFecha.Text), txtCheque.Text, _
                    "ND", Mid(txtBanco.Text, 1, 5), "", "CHEQUE DEVUELTO", -1 * Math.Abs(CDbl(txtMonto.Text)), _
                    "BAN", txtCheque.Text, "", "", "0", jytsistema.sFechadeTrabajo, jytsistema.sFechadeTrabajo, _
                    "CH", "", jytsistema.sFechadeTrabajo, "0", CodigoCliente, Mid(txtAsesor.Text, 1, 5))

        lblProgreso.Text = "1. Nota d�bito en el banco dep�sito"
        ProgressBar1.Value = 25


        '2. Coloca el cheque devuelto
        EjecutarSTRSQL(myConn, lblInfo, " insert into jsbanchedev set codban = '" & Mid(txtBanco.Text, 1, 5) & "', numcheque = '" & txtCheque.Text & "', deposito = '" & txtDeposito.Text & "', " _
                        & " prov_cli = '" & CodigoCliente & "', numcan = '" & txtCancelacion.Text & "', " _
                        & " causa = " & cmbCausa.SelectedIndex.ToString & ", monto = " & CStr(ValorNumero(txtMonto.Text)) & ", " _
                        & " fechadev = '" & FormatoFechaMySQL(txtFecha.Text) & "', ejercicio = '" & jytsistema.WorkExercise & "', id_emp = '" & jytsistema.WorkID & "' ")

        lblProgreso.Text = "2. Coloca el cheque devuelto"
        ProgressBar1.Value = 50

        If CBool(ParametroPlus(MyConn, Gestion.iBancos, "BANPARAM06")) Then
            DebitosEnCXC()
        Else
            '3. Debito en CXC por el monto del cheque
            Dim Documento As String = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMCHD", "14")
            Dim aCam() As String = {"nummov", "id_emp"}
            Dim aCad() As String = {Documento, jytsistema.WorkID}
            Dim Encontrado As Boolean = qFound(myConn, lblInfo, "jsventracob", aCam, aCad)
            While Not Encontrado
                aCad(0) = Documento
                If qFound(myConn, lblInfo, "jsventracob", aCam, aCad) Then
                    Documento = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMCHD", "14")
                Else
                    Encontrado = True
                End If
            End While

            InsertEditVENTASCXC(myConn, lblInfo, True, CodigoCliente, "ND", Documento, CDate(txtFecha.Text), FormatoHora(Now()), _
                CDate(txtFecha.Text), txtCheque.Text, "CHEQUE DEVUELTO", CDbl(txtMonto.Text), 0.0#, "", "", "", "", "", "BAN", _
                txtCheque.Text, "", "", jytsistema.sFechadeTrabajo, "", "", "", 0.0#, 0.0#, "", "", "", "", Mid(txtAsesor.Text, 1, 5), _
                Mid(txtAsesor.Text, 1, 5), "0", "0", CodigoDivision)

            lblProgreso.Text = "3. Debito en CXC por el monto del cheque"
            ProgressBar1.Value = 75

            '3. D�bito en CXC por el monto de la comisi�n + gastos administrativos
            Dim MontoComision As Double = CDbl(ParametroPlus(MyConn, Gestion.iVentas, "VENPARAM02")) + _
                (CDbl(txtMonto.Text) * CDbl(ParametroPlus(MyConn, Gestion.iVentas, "VENPARAM01").ToString) / 100)

            If MontoComision > 0 Then

                Dim CDocumento As String = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMNDB", "10")
                Dim aCamX() As String = {"nummov", "id_emp"}
                Dim aCadXC() As String = {CDocumento, jytsistema.WorkID}
                Dim iFound As Boolean = qFound(myConn, lblInfo, "jsventracob", aCam, aCad)
                While Not iFound
                    If qFound(myConn, lblInfo, "jsventracob", aCam, aCad) Then
                        iFound = True
                    Else
                        CDocumento = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMNDB", "10")
                    End If
                End While

                InsertEditVENTASCXC(myConn, lblInfo, True, CodigoCliente, "ND", CDocumento, CDate(txtFecha.Text), FormatoHora(Now()), _
                CDate(txtFecha.Text), txtCheque.Text, "COMISION CHEQUE DEVUELTO y GASTOS ADM.", MontoComision, 0.0#, "", "", "", "", "", "BAN", _
                txtCheque.Text, "", "", jytsistema.sFechadeTrabajo, "", "", "", 0.0#, 0.0#, "", "", "", "", Mid(txtAsesor.Text, 1, 5), Mid(txtAsesor.Text, 1, 5), "0", "0", CodigoDivision)

                ModificaEstatusCliente(myConn, CodigoCliente)

            End If
            lblProgreso.Text = "4. D�bito en CXC por el monto de la comisi�n + gastos administrativos"
            ProgressBar1.Value = 100

        End If

        EstatusCheque()

    End Sub


    Private Sub DebitosEnCXC()

        Dim PorcentajeComision As Double = CDbl(ParametroPlus(MyConn, Gestion.iVentas, "VENPARAM01").ToString)
        Dim MontoGastosAdministrativos As Double = CDbl(ParametroPlus(MyConn, Gestion.iVentas, "VENPARAM02"))
        Dim numFactura As String

        Dim ImporteCheque As Double = ValorNumero(txtMonto.Text)

        Dim NumeroDebito As String = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMNDB", "08")
        Dim MontoComision As Double = MontoGastosAdministrativos + (ImporteCheque * PorcentajeComision / 100)

        Dim itemChequeDevuelto As String = ParametroPlus(MyConn, Gestion.iVentas, "VENPARAM31")
        Dim descripChequeDevuelto As String = EjecutarSTRSQL_Scalar(myConn, lblInfo, " SELECT desser from jsmercatser where codser = '" & itemChequeDevuelto & "' and id_emp = '" & jytsistema.WorkID & "' ")
        Dim ivaChequeDevuelto As String = EjecutarSTRSQL_Scalar(myConn, lblInfo, " SELECT tipoiva from jsmercatser where codser = '" & itemChequeDevuelto & "' and id_emp = '" & jytsistema.WorkID & "' ")
        Dim porIVAChequeDevuelto As Double = PorcentajeIVA(myConn, lblInfo, CDate(txtFecha.Text), ivaChequeDevuelto)

        numFactura = dg.Rows.Item(0).Cells("nummov").Value

        InsertEditVENTASRenglonNOTASDEBITO(myConn, lblInfo, True, NumeroDebito, "00001", "$" & itemChequeDevuelto, _
                                           descripChequeDevuelto, ivaChequeDevuelto, "", "UND", 0.0, 1, 0.0, "", "0", _
                                            0.0, 0.0, 0.0, 0.0, 100, 0.0, 0.0, numFactura, "", 1, "", "1")


        CalculaTotalIVAVentas(myConn, lblInfo, "", "jsvenivandb", "jsvenrenndb", "numndb", NumeroDebito, "impiva", "totrendes", CDate(txtFecha.Text), "totren")

        Dim CodigoCliente As String = txtCliente.Text.Split("|")(0).Trim()

        Dim DevolucionCheque As String = txtFecha.Text
        Dim EmisionCheque As String = FormatoFecha(CDate(EjecutarSTRSQL_Scalar(myConn, lblInfo, " select fecha from jsbantracaj where tipomov = 'EN' AND formpag = 'CH' and numpag = '" & txtCheque.Text & "' and prov_cli = '" & CodigoCliente & "' and id_emp = '" & jytsistema.WorkID & "' ").ToString))
        Dim FechaIngreso As String = FormatoFecha(CDate(EjecutarSTRSQL_Scalar(myConn, lblInfo, " select ingreso from jsvencatcli where codcli = '" & CodigoCliente & "' and id_emp = '" & jytsistema.WorkID & "' ").ToString))

        Dim ChequesMes As Integer = CInt(EjecutarSTRSQL_Scalar(myConn, lblInfo, "  select count(*) chequesmes  " _
            & " from jsbanchedev " _
            & " Where " _
            & " prov_cli = '" & CodigoCliente & "' and " _
            & " month(fechadev) = " & Month(jytsistema.sFechadeTrabajo) & " and " _
            & " year(fechadev) = " & Year(jytsistema.sFechadeTrabajo) & " and " _
            & " id_emp ='" & jytsistema.WorkID & "' " _
            & " group by prov_cli "))

        Dim ChequesAno As Integer = CInt(EjecutarSTRSQL_Scalar(myConn, lblInfo, "  select count(*) chequesano  " _
            & " from jsbanchedev " _
            & " Where " _
            & " prov_cli = '" & CodigoCliente & "' and " _
            & " year(fechadev) = " & Year(jytsistema.sFechadeTrabajo) & " and " _
            & " id_emp ='" & jytsistema.WorkID & "' " _
            & " group by prov_cli "))

        Dim ChequesXX As Integer = CInt(EjecutarSTRSQL_Scalar(myConn, lblInfo, "  select count(*) cheques " _
            & " from jsbanchedev " _
            & " Where " _
            & " prov_cli = '" & CodigoCliente & "' and " _
            & " id_emp ='" & jytsistema.WorkID & "' " _
            & " group by prov_cli "))

        Dim strX As String = RellenaCadenaConCaracter("-------------------------------", "D", 80) & _
                             RellenaCadenaConCaracter("No. CHEQUE : " & txtCheque.Text & "  EMISION : " & EmisionCheque & "   DEVOLUCION : " & DevolucionCheque & "", "D", 80) & _
                             RellenaCadenaConCaracter("BANCO EMISOR : " & txtBancoEmisor.Text.Split("|")(0) & " - " & txtBancoEmisor.Text.Split("|")(1), "D", 80) & _
                             RellenaCadenaConCaracter("CAUSA : " & cmbCausa.Text.TrimEnd, "D", 80) & _
                             RellenaCadenaConCaracter("MONTO : " & FormatoNumero(ImporteCheque), "D", 80) & _
                             RellenaCadenaConCaracter("CHEQUES DEVUELTOS", "D", 80) & _
                             RellenaCadenaConCaracter("-------------------------------", "D", 80) & _
                             RellenaCadenaConCaracter("EN EL MES : " & Format(jytsistema.sFechadeTrabajo, "MM/yyyy") & "   " & FormatoEntero(ChequesMes), "D", 80) & _
                             RellenaCadenaConCaracter("EN EL A�O : " & Format(jytsistema.sFechadeTrabajo, "yyyy") & "   " & FormatoEntero(ChequesAno), "D", 80) & _
                             RellenaCadenaConCaracter("DESDE INGRESO : " & FechaIngreso & "   " & FormatoEntero(ChequesXX), "D", 80) & vbCrLf & _
                             RellenaCadenaConCaracter("                               ", "D", 80)

        EjecutarSTRSQL(myConn, lblInfo, " INSERT INTO jsvenrencom set " _
                & " numdoc = '" & NumeroDebito & "', origen = 'NDB', item = '" & "$" & itemChequeDevuelto _
                & "' , renglon = '00001', comentario = '" & strX & "', id_emp = '" & jytsistema.WorkID & "'  ")

        Dim MontoND As Double = 0.0
        Dim Impuesto As Double = 0.0
        If MontoComision > 0 Then

            'COMISION POR CHEQUE DEVUELTO
            If PorcentajeComision > 0.0# Then

                Dim itemComision As String = ParametroPlus(MyConn, Gestion.iVentas, "VENPARAM29")
                Dim descripComision As String = EjecutarSTRSQL_Scalar(myConn, lblInfo, " SELECT desser from jsmercatser where codser = '" & itemComision & "' and id_emp = '" & jytsistema.WorkID & "' ")
                Dim ivaComision As String = EjecutarSTRSQL_Scalar(myConn, lblInfo, " SELECT tipoiva from jsmercatser where codser = '" & itemComision & "' and id_emp = '" & jytsistema.WorkID & "' ")
                Dim porIVaComision As Double = PorcentajeIVA(myConn, lblInfo, CDate(txtFecha.Text), ivaComision)

                InsertEditVENTASRenglonNOTASDEBITO(myConn, lblInfo, True, NumeroDebito, "00002", "$" & itemComision, _
                                           descripComision, ivaComision, "", "UND", 0.0, 1, 0.0, "", "0", _
                                           ImporteCheque * PorcentajeComision / 100, 0.0, 0.0, 0.0, 100, _
                                           ImporteCheque * PorcentajeComision / 100, ImporteCheque * PorcentajeComision / 100, _
                                            numFactura, "", 1, "", "1")

                CalculaTotalIVAVentas(myConn, lblInfo, "", "jsvenivandb", "jsvenrenndb", "numndb", NumeroDebito, "impiva", "totrendes", CDate(txtFecha.Text), "totren")

                MontoND += ImporteCheque * PorcentajeComision / 100
                Impuesto += (ImporteCheque * PorcentajeComision / 100) * porIVaComision / 100

            End If

            'GASTOS ADMINISTRATIVOS
            If MontoGastosAdministrativos > 0 Then

                Dim itemGastos As String = ParametroPlus(MyConn, Gestion.iVentas, "VENPARAM30")
                Dim descripGastos As String = EjecutarSTRSQL_Scalar(myConn, lblInfo, " SELECT desser from jsmercatser where codser = '" & itemGastos & "' and id_emp = '" & jytsistema.WorkID & "' ")
                Dim ivaGastos As String = EjecutarSTRSQL_Scalar(myConn, lblInfo, " SELECT tipoiva from jsmercatser where codser = '" & itemGastos & "' and id_emp = '" & jytsistema.WorkID & "' ")
                Dim porIVAGastos As Double = PorcentajeIVA(myConn, lblInfo, CDate(txtFecha.Text), ivaGastos)


                InsertEditVENTASRenglonNOTASDEBITO(myConn, lblInfo, True, NumeroDebito, "00002", "$" & itemGastos, _
                                            descripGastos, ivaGastos, "", "UND", 0.0, 1, 0.0, "", "0", _
                                            MontoGastosAdministrativos, 0.0, 0.0, 0.0, 100, _
                                            MontoGastosAdministrativos, MontoGastosAdministrativos, _
                                            numFactura, "", 1, "", "1")



                CalculaTotalIVAVentas(myConn, lblInfo, "", "jsvenivandb", "jsvenrenndb", "numndb", NumeroDebito, "impiva", "totrendes", CDate(txtFecha.Text), "totren")
                MontoND += MontoGastosAdministrativos
                Impuesto += MontoGastosAdministrativos * porIVAGastos / 100

            End If

        End If

        ''Insertar Encabezado
        InsertEditVENTASEncabezadoNOTADEBITO(myConn, lblInfo, True, NumeroDebito, numFactura, CDate(txtFecha.Text), _
                                        CodigoCliente, "CHEQUE DEVUELTO, COMISION CHEQUE DEVUELTO y GASTOS ADM.", txtAsesor.Text.Split("|")(0).Trim, "00001", "00001", "", "", "A", _
                                        2, 0, 0.0, MontoND, Impuesto, MontoND + Impuesto, CDate(txtFecha.Text), 0, "", _
                                        CDate(txtFecha.Text), 0, "")

        ''Inserta la CXC
        InsertEditVENTASCXC(myConn, lblInfo, True, CodigoCliente, "ND", NumeroDebito, CDate(txtFecha.Text), FormatoHora(Now()), _
                CDate(txtFecha.Text), txtCheque.Text, "COMISION CHEQUE DEVUELTO y GASTOS ADM.", MontoND + Impuesto, Impuesto, "", "", "", "", "", "BAN", _
                txtCheque.Text, "", "", jytsistema.sFechadeTrabajo, "", "", "", 0.0#, 0.0#, "", "", "", "", Mid(txtAsesor.Text, 1, 5), Mid(txtAsesor.Text, 1, 5), "0", "0", CodigoDivision)

        Dim NDChequeDevuelto As String = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMCHD", "14")

        InsertEditVENTASCXC(myConn, lblInfo, True, CodigoCliente, "ND", NDChequeDevuelto, CDate(txtFecha.Text), FormatoHora(Now()), _
                        CDate(txtFecha.Text), txtCheque.Text, "CHEQUE DEVUELTO", CDbl(txtMonto.Text), 0.0#, "", "", "", "", "", "BAN", _
                        txtCheque.Text, "", "", jytsistema.sFechadeTrabajo, "", "", "", 0.0#, 0.0#, "", "", "", "", Mid(txtAsesor.Text, 1, 5), _
                        Mid(txtAsesor.Text, 1, 5), "0", "0", CodigoDivision)


        ''Modifica Estatus de cliente
        ModificaEstatusCliente(myConn, CodigoCliente)

        ''Imprime Nota Debito Fiscal
        Dim sRespuesta As Integer
        sRespuesta = MsgBox(" � Desea Imprimir NOTA DEBITO FISCAL ?", vbYesNo, sModulo)
        If sRespuesta = vbYes Then

            If CBool(ParametroPlus(MyConn, Gestion.iVentas, "VENNDBPA10")) Then
                '1. Imprimir Nota de Entrega Fiscal
                jytsistema.WorkBox = Registry.GetValue(jytsistema.DirReg, jytsistema.ClaveImpresorFiscal, "")
                If jytsistema.WorkBox = "" Then
                    MensajeCritico(lblInfo, "DEBE INDICAR UNA FORMA DE IMPRESION FISCAL")
                Else
                    '2. 
                    Dim nTipoImpreFiscal As Integer = TipoImpresoraFiscal(myConn, lblInfo, jytsistema.WorkBox)
                    Dim NombreDeCliente As String = txtCliente.Text.Split("|")(1).Trim()
                    Dim CodigoDeCliente As String = txtCliente.Text.Split("|")(0).Trim()
                    Dim CodigoDeAsesor As String = txtAsesor.Text.Split("|")(0).Trim()
                    Dim NombreDeAsesor As String = txtAsesor.Text.Split("|")(1).Trim()

                    Select Case nTipoImpreFiscal
                        Case 0 ' FACTURA FISCAL FORMA LIBRE
                            ' SE DIRIGE A LA IMPRESORA POR DEFECTO
                            ImprimirNotaDebitoGrafica(myConn, lblInfo, ds, NumeroDebito)
                        Case 1 'FACTURA FISCAL PRE-IMPRESA
                        Case 2, 5, 6  'IMPRESORA FISCAL TIPO ACLAS/BIXOLON
                            ImprimirNotaDebitoFiscalPP1F3(myConn, lblInfo, NumeroDebito, NumeroSERIALImpresoraFISCAL(myConn, lblInfo, jytsistema.WorkBox), _
                                NombreDeCliente, CodigoDeCliente, EjecutarSTRSQL_Scalar(myConn, lblInfo, "Select RIF from jsvencatcli where codcli = '" & CodigoDeCliente & "' and id_emp = '" & jytsistema.WorkID & "'"), _
                                EjecutarSTRSQL_Scalar(myConn, lblInfo, "Select DIRFISCAL from jsvencatcli where codcli = '" & CodigoDeCliente & "' and id_emp = '" & jytsistema.WorkID & "'"), _
                                CDate(txtFecha.Text), "0", _
                                CDate(txtFecha.Text), CodigoDeAsesor, NombreDeAsesor, nTipoImpreFiscal)
                        Case 3 'IMPRESORA FISCAL TIPO BEMATECH
                        Case 4 'IMPRESORA FISCAL TIPO EPSON/PNP
                            ImprimirNotaDebitoFiscalPnP(myConn, lblInfo, NumeroDebito, NombreDeCliente, CodigoDeCliente, _
                                EjecutarSTRSQL_Scalar(myConn, lblInfo, "Select RIF from jsvencatcli where codcli = '" & CodigoDeCliente & "' and id_emp = '" & jytsistema.WorkID & "'"), _
                                EjecutarSTRSQL_Scalar(myConn, lblInfo, "Select DIRFISCAL from jsvencatcli where codcli = '" & CodigoDeCliente & "' and id_emp = '" & jytsistema.WorkID & "'"), _
                                CDate(txtFecha.Text), "0", _
                                CDate(txtFecha.Text), CodigoDeAsesor, NombreDeAsesor)

                    End Select
                End If
                MsgBox("SE HA ENVIADO NOTA DEBITO A LA IMPRESORA FISCAL...", MsgBoxStyle.Information)

            Else
                MensajeCritico(lblInfo, "Impresi�n de NOTA DEBITO no permitida...")
            End If

        End If

    End Sub


    Private Sub ImprimirComprobante()
        ' IMPRIMIR
        Dim f As New jsBanRepParametros
        f.Cargar(TipoCargaFormulario.iShowDialog, ReporteBancos.cChequeDevuelto, "Cheque Devuelto", CodigoCliente, txtCheque.Text, CDate(txtFecha.Text))
        f = Nothing

    End Sub

    Private Sub ModificaEstatusCliente(ByVal myConn As MySqlConnection, ByVal CodigoCliente As String)

        EjecutarSTRSQL(myConn, lblInfo, " update jsvencatcli set estatus = 1 where codcli = '" & CodigoCliente & "' and id_emp = '" & jytsistema.WorkID & "' ")

        EjecutarSTRSQL(myConn, lblInfo, " insert into jsvenexpcli set codcli = '" & CodigoCliente & "' , " _
                        & " fecha = '" & FormatoFechaHoraMySQL(CDate(txtFecha.Text)) & "', " _
                        & " comentario = 'CHEQUE DEVUELTO', " _
                        & " condicion = '1', causa = '00002', tipocondicion = 0, id_emp = '" & jytsistema.WorkID & "'  ")

    End Sub

    Private Sub btnCheque_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCheque.Click
        Dim g As New jsBanProListaCheques

        g.Cargar(myConn, CDate(txtFecha.Text))

        txtMonto.Text = FormatoNumero(g.Montocheque)

        Dim aF() As String = {"codban", "id_emp"}
        Dim aFN() As String = {g.BancoDeposito, jytsistema.WorkID}
        txtBancoEmisor.Text = g.BancoCheque & "  |  " & EjecutarSTRSQL_Scalar(myConn, lblInfo, " select descrip from jsconctatab where codigo = '" & g.BancoCheque & "' and modulo = '00010' and id_emp = '" & jytsistema.WorkID & "' ")
        txtBanco.Text = g.BancoDeposito & "  |  " & qFoundAndSign(myConn, lblInfo, "jsbancatban", aF, aFN, "nomban")

        txtDeposito.Text = g.NumeroDeposito
        txtCancelacion.Text = g.NumeroCancelacion
        txtCheque.Text = g.NumeroCheque


        g = Nothing
    End Sub

    Private Sub btnFecha_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFecha.Click
        txtFecha.Text = SeleccionaFecha(CDate(txtFecha.Text), Me, btnFecha)
    End Sub

    Private Sub txtCheque_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtCheque.TextChanged
        EstatusCheque()
    End Sub

    Private Sub EstatusCheque()
        Dim aFCHC() As String = {"prov_cli", "numcheque", "id_emp"}
        Dim aFCHN() As String = {CodigoCliente, txtCheque.Text, jytsistema.WorkID}

        If qFound(myConn, lblInfo, "jsbanchedev", aFCHC, aFCHN) Then
            txtEstatus.Text = "Devuelto"
            btnImprimir.Enabled = True
            cmbCausa.SelectedIndex = CInt(qFoundAndSign(myConn, lblInfo, "jsbanchedev", aFCHC, aFCHN, "causa"))
        Else
            txtEstatus.Text = "OK!"
            btnImprimir.Enabled = False
            cmbCausa.SelectedIndex = 0
        End If
    End Sub

    Private Sub txtCancelacion_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtCancelacion.TextChanged

        Dim CondicionPago As Integer = 0 ' Credito 
        Dim dtCan As New DataTable
        Dim tblCan As String = "tblDocs"
        Dim strCan As String = "select * from jsventracob where " _
            & "COMPROBA = '" & txtCancelacion.Text & "' AND " _
            & "FOTIPO = '1' AND " _
            & "ID_EMP = '" & jytsistema.WorkID & "'"
        ds = DataSetRequery(ds, strCan, myConn, tblCan, lblInfo)
        dtCan = ds.Tables(tblCan)

        If dtCan.Rows.Count = 0 Then
            strCan = " select 'FC' tipdoccan, emision, numfac nummov, tot_fac importe from jsvenencfac where numfac = '" & txtCancelacion.Text & "' and id_emp = '" & jytsistema.WorkID & "' "
            ds = DataSetRequery(ds, strCan, myConn, tblCan, lblInfo)
            CondicionPago = 1 ' Contado 
        End If

        Dim aCampos() As String = {"tipdoccan", "emision", "nummov", "importe"}
        Dim aNombres() As String = {"TP", "Emisi�n", "Documento", "Importe"}
        Dim aAnchos() As Long = {25, 80, 100, 100}
        Dim aAlineacion() As Integer = {AlineacionDataGrid.Centro, AlineacionDataGrid.Centro, _
            AlineacionDataGrid.Izquierda, AlineacionDataGrid.Derecha}
        Dim aFormatos() As String = {"", sFormatoFecha, "", sFormatoNumero}

        IniciarTabla(dg, dtCan, aCampos, aNombres, aAnchos, aAlineacion, aFormatos, , , , False)

        Dim dtCliente As New DataTable
        Dim tblCli As String = "tblcli"
        Dim strCli As String = IIf(CondicionPago = 0, "select a.CODCLI, a.NOMBRE, a.VENDEDOR, b.codven, b.division " _
            & " from jsvencatcli a, jsventracob b where " _
            & " a.CODCLI = b.CODCLI AND " _
            & " b.COMPROBA = '" & txtCancelacion.Text & "' AND " _
            & " b.TIPOMOV IN ('CA','AB', 'NC') AND " _
            & " a.id_emp = b.ID_EMP AND " _
            & " a.ID_EMP = '" & jytsistema.WorkID & "' ", _
            " select a.codcli, a.nombre, a.vendedor, b.codven, '' division from jsvencatcli a, jsvenencfac b where " _
            & " a.codcli = b.codcli and " _
            & " b.numfac = '" & txtCancelacion.Text & "' and " _
            & " b.condpag = 1 and " _
            & " a.id_emp = b.id_emp and " _
            & " a.id_emp = '" & jytsistema.WorkID & "' ")

        ds = DataSetRequery(ds, strCli, myConn, tblCli, lblInfo)
        dtCliente = ds.Tables(tblCli)

        With dtCliente
            If .Rows.Count > 0 Then
                CodigoCliente = .Rows(0).Item("codcli").ToString
                CodigoDivision = .Rows(0).Item("division").ToString
                txtCliente.Text = .Rows(0).Item("codcli").ToString & "  |  " & .Rows(0).Item("nombre")
                Dim aFC() As String = {"codven", "id_emp"}
                Dim aFCN() As String = {.Rows(0).Item("codven").ToString, jytsistema.WorkID}
                txtAsesor.Text = .Rows(0).Item("codven").ToString & "  |  " & qFoundAndSign(myConn, lblInfo, "jsvencatven", aFC, aFCN, "apellidos") _
                                    & ", " & qFoundAndSign(myConn, lblInfo, "jsvencatven", aFC, aFCN, "nombres")
            End If
        End With


    End Sub

    Private Sub btnImprimir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnImprimir.Click
        ImprimirComprobante()
    End Sub

   
End Class
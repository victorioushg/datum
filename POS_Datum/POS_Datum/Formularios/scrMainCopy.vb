Imports System.Windows.Forms
Imports MySql.Data.MySqlClient
Imports FP_AclasBixolon

Public Class scrMainCopy

    Private Const sModulo As String = "POS Datum"
    Private Const strSQLEmpresa As String = "select * from jsconctaemp order by id_emp"

    Private myConn As New MySqlConnection(jytsistema.strConn)
    Private myCom As New MySqlCommand(strSQLEmpresa, myConn)

    Private ds As New DataSet
    Private dt As DataTable
    Private dtPerfil As DataTable
    Private ft As New Transportables()
    Private tblEmpresa As String = "tblEmpresa"

    Private tsLFecha As New ToolStripStatusLabel
    Private tsLEmpresa As New ToolStripStatusLabel
    Private tslUsuario As New ToolStripStatusLabel
    Private tsLNFecha As New ToolStripStatusLabel
    Private tsLNEmpresa As New ToolStripStatusLabel
    Private tslNUsuario As New ToolStripStatusLabel
    Private tsTituloCaja As New ToolStripStatusLabel
    Private tstxtCaja As New ToolStripStatusLabel
    Private tsTituloImpresora As New ToolStripStatusLabel
    Private tstxtImpresora As New ToolStripStatusLabel

    Private numFacturaTemporal As String = ""

    Private strSQLMov As String = ""
    Private nTablaRenglones As String = "tblRenglones"
    Private dtRenglones As DataTable
    Private nPosicionRenglon As Long

    Private CodigoCliente As String = ""
    Private DescuentoCliente As Double = 0.0

    Private CantidadRenglon As Double = 1.0
    Private LongitudBarrasArticulo As Integer = 0
    Private LongitudBarrasColor As Integer = 0
    Private LongitudBarrasTalla As Integer = 0
    Private LongitudBarrasPeso As Integer = 0
    Private LongitudBarraPrecio As Integer = 0
    Private TarifaPrecios As String = "A"
    Private AlmacenSalida As String = ""
    Private CondicionDePago As CondicionPago = CondicionPago.iContado
    Private TipoCredito As Integer = 0
    Private FechaVencimiento As Date = MyDate
    Private Disponibilidad As Double = 0.0

    Private contadorCintillo As Integer = 0
    Private counterTick As Integer = 0

    Private myPerfil As New Perfil
    Private CodigoVendedor As String = ""
    Private NombreVendedor As String = ""
    Private nImpresora As String = ""
    Private numSerialFiscal As String = ""

    Private condicionIVAEspecial As Boolean = False


    Private IB As New AclasBixolon

    Private bRet As Boolean


    Private Sub scrMain_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed

        InsertarAuditoria(myConn, MovAud.iSalir, sModulo, "")
        dt = Nothing
        ds = Nothing
        myCom = Nothing
        myConn.Close()
        myConn = Nothing

    End Sub

    Private Sub scrMain_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown

        Select Case e.KeyCode

            Case Keys.Multiply
                CambiarCantidad()
            Case Keys.F1
                ft.mensajeInformativo("Ayuda No habilitada")
            Case Keys.F4 'Agregar Renglón
                If IIf(EsRIF(txtRIFCI.Text.Trim), validarRif(txtRIFCI.Text.Trim), validarCI(txtRIFCI.Text.Trim.Split("-")(0) + "-" + txtRIFCI.Text.Trim.Split("-")(1).Trim)) Then
                    If txtNombre.Text.Trim <> "" Then
                        If cmbPerfil.Text = "" Then
                            ft.mensajeCritico("DEBE INDICAR UN PERFIL VALIDO")
                        Else
                            Dim f As New jsGenRenglonesMovimientos
                            f.RIFCI = txtRIFCI.Text
                            f.Agregar(myConn, ds, dtRenglones, "PVE", numFacturaTemporal, jytsistema.sFechadeTrabajo, AlmacenSalida, TarifaPrecios, CodigoCliente)
                            nPosicionRenglon = f.Apuntador
                            AsignaMov(nPosicionRenglon, True)
                            CalculaTotales()
                            f = Nothing
                        End If
                    Else
                        ft.mensajeCritico("DEBE INDICAR UN NOMBRE DE CLIENTE VALIDO")
                    End If
                Else
                    ft.mensajeCritico("DEBE INDICAR UNA CI/RIF DE CLIENTE VALIDO")
                End If
            Case Keys.F5 'Modificar Renglón
                If IIf(EsRIF(txtRIFCI.Text.Trim), validarRif(txtRIFCI.Text.Trim), validarCI(txtRIFCI.Text.Trim.Split("-")(0) + "-" + txtRIFCI.Text.Trim.Split("-")(1).Trim)) Then
                    If txtNombre.Text.Trim <> "" Then
                        If dtRenglones.Rows.Count > 0 Then
                            Dim g As New dtSupervisor
                            g.Cargar(myConn, ds)
                            If g.DialogResult = Windows.Forms.DialogResult.OK Then
                                Dim f As New jsGenRenglonesMovimientos
                                f.RIFCI = txtRIFCI.Text
                                f.Apuntador = nPosicionRenglon
                                f.Editar(myConn, ds, dtRenglones, "PVE", numFacturaTemporal, jytsistema.sFechadeTrabajo, AlmacenSalida, TarifaPrecios, CodigoCliente)
                                nPosicionRenglon = f.Apuntador
                                AsignaMov(nPosicionRenglon, True)
                                CalculaTotales()
                                f = Nothing
                            End If
                            g.Dispose()
                            g = Nothing
                        End If
                    Else
                        ft.mensajeCritico("DEBE INDICAR UN NOMBRE DE CLIENTE VALIDO")
                    End If
                Else
                    ft.mensajeCritico("DEBE INDICAR UNA CI/RIF DE CLIENTE VALIDO")
                End If
            Case Keys.F6

                Dim f As New dtSupervisor
                f.Cargar(myConn, ds)
                If f.DialogResult = Windows.Forms.DialogResult.OK Then
                    EliminaFila()
                End If
                f.Dispose()
                f = Nothing
            Case Keys.F8
                CerrarFactura()
            Case Keys.Enter

                If IIf(EsRIF(txtRIFCI.Text.Trim), validarRif(txtRIFCI.Text.Trim), validarCI(txtRIFCI.Text.Trim.Split("-")(0) + "-" + txtRIFCI.Text.Trim.Split("-")(1).Trim)) Then
                    If txtNombre.Text.Trim <> "" Then
                        If cmbPerfil.Text = "" Then
                            ft.mensajeCritico("DEBE INDICAR UN PERFIL VALIDO")
                        Else
                            If txtBarras.BackColor = Color.LightGoldenrodYellow Then
                                CantidadRenglon = ValorCantidad(Replace(txtBarras.Text, "*", ""))
                                txtBarras.BackColor = Color.PaleGreen
                                EnfocarTextoTP(txtBarras)
                            Else
                                IncluirRenglon(txtBarras.Text)
                                CalculaTotales()
                            End If
                        End If
                    Else
                        ft.mensajeCritico("DEBE INDICAR UN NOMBRE DE CLIENTE VALIDO")
                    End If
                Else
                    ft.mensajeCritico("DEBE INDICAR UNA CI/RIF DE CLIENTE VALIDO")
                End If
        End Select
        e.Handled = True
    End Sub

    Private Sub scrMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load


        nGestion = Gestion.iPuntosdeVentas

        Try

            myConn.Open()
            ds = DataSetRequery(ds, strSQLEmpresa, myConn, tblEmpresa, lblInfo)
            dt = ds.Tables(tblEmpresa)
            InsertarAuditoria(myConn, MovAud.ientrar, sModulo, "")

            If dt.Rows.Count > 0 Then
                With dt
                    jytsistema.WorkID = .Rows(0).Item("id_emp")
                    jytsistema.WorkName = IIf(IsDBNull(.Rows(0).Item("nombre")), "", .Rows(0).Item("nombre"))
                    jytsistema.WorkExercise = ""
                End With
            End If

            ValoresIniciales()
            IniciarUnidadesDeMedida()
            AsignarTooltips()
            Iniciar_Apagado()
            Select Case TipoImpresoraFiscal(myConn, jytsistema.WorkBox)
                Case 2, 5, 6, 7
                    'bRet = IB.abrirPuerto(PuertoImpresoraFiscal(myConn, lblInfo, jytsistema.WorkBox))
                    'If Not bRet Then ft.mensajeCritico("PUERTO DE IMPRESORA FISCAL NO ESTA DISPONIBLE. POR FAVOR VERIFIQUE...")
                Case 4
                    PFreset()
            End Select

        Catch ex As MySqlException
            ft.mensajeCritico("Error en conexión de base de datos: " & ex.Message)
        End Try



    End Sub
    Private Sub AsignarTooltips()

        'Menu Barra 
        C1SuperTooltip1.SetToolTip(txtBarras, "<B>Indique o Escaneé</B> un código de barras válido")
        C1SuperTooltip1.SetToolTip(btnPor, "<B>Multiplicador</B> de cantidades en renglón")
        C1SuperTooltip1.SetToolTip(btnAgregarMovimiento, "<B>Agrega renglón</B> de forma manual")
        C1SuperTooltip1.SetToolTip(btnEditarMovimiento, "<B>Edita</B> el renglón actual")
        C1SuperTooltip1.SetToolTip(btnEliminarMovimiento, "<B>Elimina</B> el renglon actual")
        C1SuperTooltip1.SetToolTip(btnDescuentosRenglon, "<B>Asigna descuentos</B> en el renglón actual")
        C1SuperTooltip1.SetToolTip(btnPrimerMovimiento, "ir al <B>primer renglón</B>")
        C1SuperTooltip1.SetToolTip(btnAnteriorMovimiento, "ir al renglón <B>anterior</B>")
        C1SuperTooltip1.SetToolTip(btnSiguienteMovimiento, "ir al renglón <B>siguiente</B>")
        C1SuperTooltip1.SetToolTip(btnUltimoMovimiento, "ir al <B>último</B> renglon ")
        C1SuperTooltip1.SetToolTip(btnEliminarFactura, "<B>Salir y eliminar</B> la factura/devolución actual")
        C1SuperTooltip1.SetToolTip(btnNotas, "<B>Baja renglones</B> de una o varias notas de entrega")

        C1SuperTooltip1.SetToolTip(btnCliente, "<B>escojer y/o buscar</B> clientes")
        C1SuperTooltip1.SetToolTip(btnCliente, "<B>escojer y/o buscar</B> vendedor de piso")

        C1SuperTooltip1.SetToolTip(btnDescuentosFactura, "<B>Asignar descuentos</B> por factura")
        C1SuperTooltip1.SetToolTip(btnIVA, "<B>Verificar</B> impuesto al valor agregado (IVA) de esta factura/devolución")

    End Sub
    Private Sub IniciarUnidadesDeMedida()

        Dim dtUM As DataTable = ft.AbrirDataTable(ds, "tblUnidadDeMedida", myConn, " select * from jsconctatab where " _
                                                  & " modulo = '00035' and " _
                                                  & " id_emp = '" & jytsistema.WorkID & "' " _
                                                  & " order by codigo ")
        Dim iCont As Integer = 0
        For Each nrow As DataRow In dtUM.Rows
            With nrow
                ReDim Preserve aUnidad(iCont)
                ReDim Preserve aUnidadAbreviada(iCont)
                aUnidadAbreviada(iCont) = .Item("CODIGO")
                aUnidad(iCont) = .Item("DESCRIP")
                iCont += 1
            End With
        Next

    End Sub
    Private Sub Iniciar_Apagado()
        HabilitarBotones()
        ft.visualizarObjetos(False, grpFactura)
        ft.habilitarObjetos(False, False, btnDevolver, btnPagar, btnSubir, btnSubirApartado)
        Me.KeyPreview = False
    End Sub
    Private Sub ApagarFactura()

        HabilitarBotones()
        ft.habilitarObjetos(False, False, btnSalir, btnAgregar, btnBajar, _
                                 btnReporteX, btnReporteZ, btnMenuImpresora, btnRecambios, _
                                 btnSubirApartado, btnConfigurar, btnRetencionIVA, btnControlEfectivo)
        Me.KeyPreview = True
        EnfocarTextoM(txtRIFCI)
    End Sub
    Private Sub HabilitarBotones()
        ft.habilitarObjetos(True, False, btnSalir, btnAgregar, btnDevolver, btnPagar, btnSubir, btnBajar, _
                                 btnReporteX, btnReporteZ, btnMenuImpresora, btnMercancias, btnRecambios, _
                                 btnSubirApartado, btnBajarApartado, btnConfigurar, btnRetencionIVA, btnControlEfectivo)
    End Sub


    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        Dim aCampos() As String = {"codcaj", "id_emp"}
        Dim aStrings() As String = {jytsistema.WorkBox, jytsistema.WorkID}
        Dim NombreCaja As String = qFoundAndSign(myConn, lblInfo, "jsvencatcaj", aCampos, aStrings, "descrip")

        tsLNFecha.Font = New Font("Arial", 8, FontStyle.Bold)
        tsLNFecha.Text = "  Fecha : "
        tsLFecha.Text = Format(jytsistema.sFechadeTrabajo, "D")
        tsLNEmpresa.Font = New Font("Arial", 8, FontStyle.Bold)
        tsLNEmpresa.Text = "  Empresa : "
        tsLEmpresa.Text = jytsistema.WorkName
        tslNUsuario.Font = New Font("Arial", 8, FontStyle.Bold)
        tslNUsuario.Text = "  Cajero : "
        tslUsuario.Text = " " & jytsistema.sNombreUsuario
        tsTituloCaja.Font = New Font("Arial", 8, FontStyle.Bold)
        tsTituloCaja.Text = "  Caja : "
        tstxtCaja.Text = jytsistema.WorkBox & " " & NombreCaja
        tsTituloImpresora.Font = New Font("Arial", 8, FontStyle.Bold)
        tsTituloImpresora.Text = "  Impresora : "
        tstxtImpresora.Text = nImpresora


        If myConn.State = ConnectionState.Open Then
            If counterTick > CInt(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM40").ToString) Then
                If CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM37")) Then

                    contadorCintillo += 1

                    Dim dtCintillo As New DataTable
                    Dim nTableCintillo As String = "tblcintillo"
                    ds = DataSetRequery(ds, " select * from jsconctatab where modulo = '" & FormatoTablaSimple(Modulo.iFrasesCintillo) & "' and id_emp = '" & jytsistema.WorkID & "' order by codigo ", myConn, nTableCintillo, lblInfo)
                    dtCintillo = ds.Tables(nTableCintillo)

                    If contadorCintillo >= dtCintillo.Rows.Count Then contadorCintillo = 0
                    If dtCintillo.Rows.Count > 0 Then
                        With dtCintillo.Rows(contadorCintillo)
                            lblCintillo.Text = .Item("descrip")
                        End With
                    End If

                    dtCintillo.Dispose()
                    dtCintillo = Nothing

                End If
                counterTick = 0
            Else
                counterTick += 1
            End If
        End If



        tsLFecha.TextAlign = ContentAlignment.MiddleLeft
        tsLEmpresa.TextAlign = ContentAlignment.MiddleLeft
        tslUsuario.TextAlign = ContentAlignment.MiddleLeft
        tsLNFecha.TextAlign = ContentAlignment.MiddleLeft
        tsLNEmpresa.TextAlign = ContentAlignment.MiddleLeft
        tslNUsuario.TextAlign = ContentAlignment.MiddleLeft


        With StatusStrip
            .Items.Clear()
            .Items.Add(tsLNFecha)
            .Items.Add(tsLFecha)
            .Items.Add(tsLNEmpresa)
            .Items.Add(tsLEmpresa)
            .Items.Add(tslNUsuario)
            .Items.Add(tslUsuario)
            .Items.Add(tsTituloCaja)
            .Items.Add(tstxtCaja)
            .Items.Add(tsTituloImpresora)
            .Items.Add(tstxtImpresora)

        End With

    End Sub
    '////////////////////////////////////////
    Private Sub CerrarFactura()
        Dim f As New dtSupervisor
        f.Cargar(myConn, ds)
        If f.DialogResult = Windows.Forms.DialogResult.OK Then
            EliminarFactura(numFacturaTemporal, numSerialFiscal)
            ft.visualizarObjetos(False, grpFactura)
            Iniciar_Apagado()
        End If
        f.Close()
        f = Nothing
    End Sub

    Private Sub EliminarFactura(ByVal CodigoFactura As String, ByVal NumeroSerialFiscal As String)

        ft.Ejecutar_strSQL(myConn, " delete from jsvenencpos where numfac = '" & CodigoFactura & "' and ejercicio = '" & jytsistema.WorkExercise & "' and id_emp = '" & jytsistema.WorkID & "'")
        ft.Ejecutar_strSQL(myConn, " delete from jsvenrenpos where numfac = '" & CodigoFactura & "' and ejercicio = '" & jytsistema.WorkExercise & "' and id_emp = '" & jytsistema.WorkID & "'")
        ft.Ejecutar_strSQL(myConn, " delete from jsvenivapos where numfac = '" & CodigoFactura & "' and ejercicio = '" & jytsistema.WorkExercise & "' and id_emp = '" & jytsistema.WorkID & "'")
        ft.Ejecutar_strSQL(myConn, " delete from jsvendespos where numfac = '" & CodigoFactura & "' and ejercicio = '" & jytsistema.WorkExercise & "' and id_emp = '" & jytsistema.WorkID & "'")
        ft.Ejecutar_strSQL(myConn, " delete from jsvenforpag where numfac = '" & CodigoFactura & "' and origen = 'PVE' and id_emp = '" & jytsistema.WorkID & "'")
        ft.Ejecutar_strSQL(myConn, " delete from jsventrapos where nummov = '" & CodigoFactura & "' and caja = '" & jytsistema.WorkBox & "' and ejercicio = '" & jytsistema.WorkExercise & "' and id_emp = '" & jytsistema.WorkID & "'")
        ft.Ejecutar_strSQL(myConn, " delete from jsmertramer where numdoc = '" & CodigoFactura & "' and origen = 'PVE' and ejercicio = '" & jytsistema.WorkExercise & "' and id_emp = '" & jytsistema.WorkID & "'")
        If ExisteTabla(myConn, jytsistema.WorkDataBase, "jsmermovcatcol") Then ft.Ejecutar_strSQL(myConn, " delete from jsmermovcatcol where nummov = '" & CodigoFactura & "' and tipomov = 'SA' and origen = 'PVE' and id_emp = '" & jytsistema.WorkID & "'")

        InsertarAuditoria(myConn, MovAud.iEliminar, sModulo, CodigoFactura)
        ft.mensajeInformativo("FACTURA ELIMINADA...        ")

    End Sub


    Private Sub IniciarFactura(ByVal NumeroFacturaI As String)

        RellenaPerfil(jytsistema.WorkBox)

        If NumeroFacturaI = "" Then

            numFacturaTemporal = Contador(myConn, lblInfo, Gestion.iPuntosdeVentas, "POSNUMTMP", "06")
            grpEncab.Text = numFacturaTemporal
            txtRIFCI.Text = ""
            txtNombre.Text = ""
            txtDireccion.Text = ""
            txtTelefono.Text = ""

            txtBarras.Text = ""

        Else

            Dim aFld() As String = {"numfac", "id_emp"}
            Dim aStr() As String = {NumeroFacturaI, jytsistema.WorkID}
            numFacturaTemporal = NumeroFacturaI

            txtRIFCI.Text = qFoundAndSign(myConn, lblInfo, "jsvenencpos", aFld, aStr, "RIF")

            txtBarras.Text = ""

        End If

        ft.iniciarTextoObjetos(Transportables.tipoDato.Cantidad, txtPesoTotal)
        ft.iniciarTextoObjetos(Transportables.tipoDato.Numero, txtSubtotal, txtDescuentos, txtIVA, txtTotal, _
                                txtAbonado, txtACancelar)

        ft.habilitarObjetos(True, True, txtRIFCI, txtNombre, txtTelefono, txtDireccion, btnCliente, btnVendedor, btnIVA, btnDescuentosFactura)
        ft.habilitarObjetos(False, True, txtVendedorPiso, txtVendedorPisoNombre, txtPesoTotal, _
                         txtSubtotal, txtDescuentos, txtIVA, txtTotal, txtAbonado, txtACancelar)

        '//////// ENCABEZADO
        CondicionDePago = CondicionPago.iContado

        '//////// RENGLONES
        strSQLMov = "select * from jsvenrenpos " _
            & " where " _
            & " numfac  = '" & numFacturaTemporal & "' and " _
            & " ejercicio = '" & jytsistema.WorkExercise & "' and " _
            & " id_emp = '" & jytsistema.WorkID & "' order by RENGLON "

        dtRenglones = ft.AbrirDataTable(ds, nTablaRenglones, myConn, strSQLMov)

        Dim aCampos() As String = {"renglon.Renglón.70.C.", _
                                   "descrip.Descripción.400.I.", _
                                   "iva.IVA.45.C.", _
                                   "cantidad.Cantidad.90.D.Cantidad", _
                                   "unidad.UND.45.C.", _
                                   "precio.Precio Unitario.100.D.Numero", _
                                   "des_cli.Dscto Cliente.80.D.Numero", _
                                   "des_art.Dscto Artículo.80.D.Numero", _
                                   "des_ofe.Dscto Oferta.80.D.Numero", _
                                   "totrendes.Total renglón.120.D.Numero", _
                                   "sada..10.I."}

        ft.IniciarTablaPlus(dg, dtRenglones, aCampos, , , New Font("Consolas", 12, FontStyle.Bold), , 22)
        If dtRenglones.Rows.Count > 0 Then nPosicionRenglon = 0
        AsignaMov(nPosicionRenglon, True)

        CalculaTotales()


    End Sub
    Private Sub ValoresIniciales()

        CantidadRenglon = 1
        LongitudBarrasArticulo = CInt(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM14"))
        LongitudBarrasTalla = CInt(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM15"))
        LongitudBarrasColor = CInt(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM16"))


        ft.visualizarObjetos(False, grpDisponibilidad, IIf(CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM37")), grpDisponibilidad, lblCintillo))

        Select Case TipoImpresoraFiscal(myConn, jytsistema.WorkBox)

            Case 2, 5, 6, 7
                bRet = IB.abrirPuerto(PuertoImpresoraFiscal(myConn, lblInfo, jytsistema.WorkBox))
                If bRet Then
                    Select Case TipoImpresoraFiscal(myConn, jytsistema.WorkBox)
                        Case 2, 6
                            numSerialFiscal = IB.UltimoDocumentoFiscal(AclasBixolon.tipoDocumentoFiscal.numRegistro)
                        Case 5
                            numSerialFiscal = IB.UltimoDocumentoFiscal(AclasBixolon.tipoDocumentoFiscal.NR_SRP350)
                        Case 7
                            numSerialFiscal = IB.UltimoDocumentoFiscal(AclasBixolon.tipoDocumentoFiscal.NR_SRP812)
                    End Select
                    IB.cerrarPuerto()
                    nImpresora = ft.DevuelveScalarCadena(myConn, "  SELECT a.impre_fiscal " _
                                                         & " FROM jsvencatcaj a " _
                                                         & " WHERE " _
                                                         & " a.codcaj = '" & jytsistema.WorkBox & "' AND " _
                                                         & " a.id_emp = '" & jytsistema.WorkID & "'") & " | Serial : " _
                                                         & numSerialFiscal & " | Puerto : " & PuertoImpresoraFiscal(myConn, lblInfo, jytsistema.WorkBox)

                End If

                If numSerialFiscal.Trim.Equals("") Then
                    ft.mensajeAdvertencia("Número serial impresora fiscal NO VALIDO. no es posible FACTURAR. VERIFIQUE por favor... ")
                End If

            Case Else
                nImpresora = ft.DevuelveScalarCadena(myConn, "  SELECT CONCAT(b.codigo, '  | Serial : ', b.maquinafiscal, '  | Puerto : ' , b.puerto) " _
                                                         & " FROM jsvencatcaj a " _
                                                         & " LEFT JOIN jsconcatimpfis b ON (a.impre_fiscal = b.codigo AND a.id_emp = b.id_emp ) " _
                                                         & " WHERE " _
                                                         & " a.codcaj = '" & jytsistema.WorkBox & "' AND " _
                                                         & " a.id_emp = '" & jytsistema.WorkID & "'")
        End Select





    End Sub
    Private Sub RellenaPerfil(CodigoCaja As String)

        Dim nTablePerfil As String = "tblPerfil" & ft.NumeroAleatorio(100000)

        Dim strSQLPer As String = " select a.codcaj, concat(b.codper,'|', b.descrip) perfil, b.codper " _
                                  & " from jsvenpervencaj a " _
                                  & " left join jsvenperven b on (a.codper = b.codper and a.id_emp = b.id_emp) " _
                                  & " where " _
                                  & " a.codcaj = '" & jytsistema.WorkBox & "' and " _
                                  & " a.id_emp = '" & jytsistema.WorkID & "' "

        dtPerfil = ft.AbrirDataTable(ds, nTablePerfil, myConn, strSQLPer)
        Dim aPerfil() As String = {}
        Dim iCont As Integer = -1
        For Each nRow As DataRow In dtPerfil.Rows
            iCont += 1
            aPerfil = InsertArrayItemStringPlus(aPerfil, iCont, nRow.Item("PERFIL"))
        Next
        ft.RellenaCombo(aPerfil, cmbPerfil)

    End Sub
    Private Sub AsignaMov(ByVal nRow As Long, ByVal Actualiza As Boolean)

        If Actualiza Then
            ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
            dtRenglones = ds.Tables(nTablaRenglones)
        End If

        Dim c As Integer = CInt(nRow)
        If c >= 0 AndAlso dtRenglones.Rows.Count > 0 Then
            Me.BindingContext(ds, nTablaRenglones).Position = c
            dg.Refresh()
            dg.CurrentCell = dg(0, c)
        End If
        MostrarItemsEnMenuBarra(MenuBarraRenglon, c, dtRenglones.Rows.Count)

        ft.habilitarObjetos(IIf(dtRenglones.Rows.Count > 0, False, True), True, txtRIFCI, btnCliente, txtNombre, txtTelefono, txtDireccion)
        ft.habilitarObjetos(IIf(dtRenglones.Rows.Count > 0, False, IIf(CodigoCliente = "00000000", False, True)), True, cmbPerfil)

    End Sub
    '////////////////////////////////////////
    Private Sub btnSalir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSalir.Click

        Dim f As New dtSupervisor
        f.Cargar(myConn, ds)
        If f.DialogResult = Windows.Forms.DialogResult.OK Then End
        f.Close()
        f = Nothing

    End Sub

    Private Sub btnAgregar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregar.Click
        ft.visualizarObjetos(True, grpFactura)
        IniciarFactura("")
        ApagarFactura()
    End Sub

    Private Sub btnEliminarFactura_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEliminarFactura.Click
        CerrarFactura()
    End Sub

    Private Sub txtBarras_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtBarras.KeyDown
        Me.KeyPreview = True
    End Sub

    Private Sub txtBarras_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtBarras.LostFocus
        txtBarras.BackColor = Color.LightPink
    End Sub
    Private Sub btnPor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPor.Click
        CambiarCantidad()
    End Sub
    Private Sub CambiarCantidad()
        txtBarras.BackColor = Color.LightGoldenrodYellow
        EnfocarTextoTP(txtBarras)
        Me.KeyPreview = False
        txtBarras.Text = ""
    End Sub
    Private Sub txtBarras_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtBarras.Click
        txtBarras.BackColor = Color.PaleGreen
    End Sub
    Private Sub txtBarras_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtBarras.GotFocus
        If txtBarras.BackColor = Color.LightPink Then txtBarras.BackColor = Color.PaleGreen
    End Sub
    Private Sub IncluirRenglon(ByVal CodigoBarras As String)

        Dim Descuento_articulo As Double = 0.0
        Dim Descuento_oferta As Double = 0.0
        Dim Descuento_cliente As Double = 0.0

        Dim CodBarras As String = ""
        Dim CodTalla As String = ""
        Dim CodColor As String = ""


        Dim nomColor As String = ""
        If Trim(CodigoBarras) <> "" Then
            If CodigoBarras.Length <= LongitudBarrasArticulo + LongitudBarrasTalla + LongitudBarrasColor Then

                CodBarras = IIf(LongitudBarrasArticulo > 0, Trim(UCase(Mid(CodigoBarras, 1, LongitudBarrasArticulo))), CodigoBarras)
                If CodBarras.Length = 12 And Mid(CodBarras, 1, 2) = "01" Then
                    CantidadRenglon = CDbl(Mid(CodBarras, 7, 5)) / 1000
                    CodBarras = Mid(CodBarras, 1, 6)
                End If

                CodTalla = UCase(Mid(CodigoBarras, LongitudBarrasArticulo + 1, LongitudBarrasTalla))
                CodColor = UCase(Mid(CodigoBarras, LongitudBarrasArticulo + LongitudBarrasTalla + 1, LongitudBarrasColor))

                Dim afldColor() As String = {"codcol", "id_emp"}
                Dim aStrColor() As String = {CodColor, jytsistema.WorkID}
                If ExisteTabla(myConn, jytsistema.WorkDataBase, "jsmercatcol") Then _
                    nomColor = qFoundAndSign(myConn, lblInfo, "jsmercatcol", afldColor, aStrColor, "descrip")

                Dim CodigoMercancia As String = getCodigoMercancia(myConn, CodBarras)
                TarifaPrecios = getTarifaPrecioMercancia(myConn, myPerfil, CodBarras)

                Dim aFld() As String = {"CODART", "id_emp"}
                Dim aStr() As String = {CodigoMercancia, jytsistema.WorkID}

                If ft.DevuelveScalarEntero(myConn, " SELECT COUNT(*) FROM jsmerctainv WHERE CODART = '" & CodigoMercancia & "' AND ID_EMP = '" & jytsistema.WorkID & "' ") > 0 Then

                    Dim MercanciaInactiva As Boolean = ft.DevuelveScalarBooleano(myConn, "Select estatus from jsmerctainv where codart = '" & CodigoMercancia & "' and id_emp = '" & jytsistema.WorkID & "' ")
                    If MercanciaInactiva Then
                        ft.mensajeCritico("ESTA MERCANCIA POSEE ESTATUS DE INACTIVA POR FAVOR DIRIGASE AL SUPERVISOR ...")
                        Return
                    End If

                    '*////////////////////////////////////////////
                    '*///VALIDA EXISTENCIAS DE MERCANCIAS/////////
                    Dim UnidadVentaPOS As String = CStr(ft.DevuelveScalarCadena(myConn, " SELECT UNIDADDETAL FROM jsmerctainv WHERE CODART = '" & CodigoMercancia & "' AND ID_EMP = '" & jytsistema.WorkID & "' "))
                    If CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM21")) Then

                        Dim CantidadMovimiento As Double = ft.DevuelveScalarDoble(myConn, " SELECT SUM( IF ( b.uvalencia IS NULL, a.cantidad, a.cantidad / b.equivale ) ) " _
                                                                & " FROM jsvenrenpos a " _
                                                                & " LEFT JOIN jsmerequmer b ON (a.item = b.codart AND a.unidad = b.uvalencia AND a.id_emp = b.id_emp) " _
                                                                & " WHERE " _
                                                                & " a.numfac = '" & numFacturaTemporal & "' AND " _
                                                                & " a.item = '" & CodigoMercancia & "' AND " _
                                                                & " a.id_emp = '" & jytsistema.WorkID & "' group by a.numfac")

                        Dim xCant As Double = CantidadRenglon / Equivalencia(myConn, CodigoMercancia, UnidadVentaPOS) + CantidadMovimiento
                        Dim xExis As Double = ExistenciasEnAlmacenes(myConn, CodigoMercancia, AlmacenSalida)
                        If xCant > xExis Then
                            If AlmostEqualPLUS(myConn, xCant, xExis) And (0 <= (xCant - xExis) And (xCant - xExis) < 1) Then
                                ' ft.mensajeInformativo("CASI IGUAL")
                            Else
                                ft.mensajeCritico("Cantidad es mayor a la existente en el almacén " & AlmacenSalida & " ...")
                                Return
                            End If

                        End If
                    End If


                    Dim nEquivale As Double = Equivalencia(myConn, CodigoMercancia, UnidadVentaPOS)
                    Dim PesoRenglon As Double = IIf(Mid(CodBarras, 1, 2) = "01", CantidadRenglon, CantidadRenglon * ft.DevuelveScalarDoble(myConn, " SELECT PESOUNIDAD FROM jsmerctainv WHERE CODART = '" & CodigoMercancia & "' AND ID_EMP = '" & jytsistema.WorkID & "' ")) / IIf(nEquivale = 0, 1, nEquivale)
                    Dim PrecioUnitario As Double = Math.Round(ft.DevuelveScalarDoble(myConn, " SELECT PRECIO_" & TarifaPrecios & " FROM jsmerctainv WHERE CODART = '" & CodigoMercancia & "' AND ID_EMP = '" & jytsistema.WorkID & "' ") / IIf(nEquivale = 0, 1, nEquivale), 2)
                    Dim DescuentoArticulo As Double = ft.DevuelveScalarDoble(myConn, " SELECT DESC_" & TarifaPrecios & " FROM jsmerctainv WHERE CODART = '" & CodigoMercancia & "' AND ID_EMP = '" & jytsistema.WorkID & "' ")
                    Dim DescuentoOferta As Double = PorcentajeDescuentoOferta(myConn, jytsistema.sFechadeTrabajo, CodigoMercancia, _
                                                                              UnidadVentaPOS, _
                                                                              CantidadRenglon, TarifaPrecios, lblInfo)
                    DescuentoCliente = ft.DevuelveScalarDoble(myConn, " select DES_CLI from jsvencatcli where codcre = '0' and codcli =  '" + CodigoCliente + "' and id_emp = '" + jytsistema.WorkID + "' ")

                    Select Case myPerfil.Descuento
                        Case 0, 2 'SIN DESCUENTO 
                            Descuento_articulo = 0.0
                            Descuento_oferta = 0.0
                            Descuento_cliente = 0.0
                        Case 1 'POR DEFECTO
                            Descuento_articulo = DescuentoArticulo / 100
                            Descuento_oferta = DescuentoOferta / 100
                            Descuento_cliente = DescuentoCliente / 100
                        Case Else
                            Descuento_articulo = 0.0
                            Descuento_oferta = 0.0
                            Descuento_cliente = 0.0
                    End Select

                    If UnidadVentaPOS = "0" Then
                        ft.mensajeCritico("UNIDAD DE VENTA INVALIDA. POR FAVOR INTRODUZCA ESTE ITEM DE NUEVO...")
                        Return
                    End If
                    If CantidadRenglon = 0.0 Then
                        ft.mensajeCritico("CANTIDAD INVALIDA. POR FAVOR INTRODUZCA ESTE ITEM DE NUEVO...")
                        Return
                    End If

                    If PrecioUnitario <= 0.0 Then
                        ft.mensajeCritico("PRECIO UNITARIO INVALIDO. POR FAVOR INTRODUZCA ESTE ITEM DE NUEVO...")
                        Return
                    End If


                    If CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM33")) Then

                        Dim MercanciaRegulada As Boolean = ft.DevuelveScalarBooleano(myConn, "Select regulado from jsmerctainv where codart = '" & CodigoMercancia & "' and id_emp = '" & jytsistema.WorkID & "' ")
                        Dim CodigoProducto As String = ft.DevuelveScalarCadena(myConn, "Select CODJER from jsmerctainv where codart = '" & CodigoMercancia & "' and id_emp = '" & jytsistema.WorkID & "' ").ToString.Replace(".", "")

                        If CodigoProducto.Trim() <> "" Then      ''MercanciaRegulada Or 

                            Dim TipoRazon As String = txtRIFCI.Text.Split("-")(0).ToString
                            Dim Documento As String = txtRIFCI.Text.Split("-")(1).ToString.Replace("_", "").Replace(" ", "") & _
                                IIf(EsRIF(txtRIFCI.Text), txtRIFCI.Text.Split("-")(2), "0")

                            Dim CantidadMaximaDeVenta As Integer = ft.DevuelveScalarEntero(myConn, _
                                                                        " select CANTIDAD from jsmercodsica WHERE CODIGO = '" & CodigoProducto & "' ")
                            Dim frecuenciaDias As Integer = ft.DevuelveScalarEntero(myConn, _
                                                                        " select FRECUENCIA from jsmercodsica WHERE CODIGO = '" & CodigoProducto & "' ")


                            'Dim EstatusClienteCADIP As Integer = CInt(EjecutarSTRSQL_Scalar(myConn, lblInfo, _
                            '                                            " select ESTATUS from jsvenCADIPposX WHERE TIPORAZON = '" & TipoRazon _
                            '                                            & "' AND DOCUMENTO = '" & Documento _
                            '                                            & "' AND CODIGO_PRODUCTO = '" & CodigoProducto _
                            '                                            & "' AND ID_EMP = '" & jytsistema.WorkID & "' "))

                            'If EstatusClienteCADIP > 1 Then

                            '    Dim DescripcionProductoBloqueado As String = qFoundAndSign(myConn, lblInfo, "jsmerctainv", aFld, aStr, "nomart")
                            '    Dim FechaProximaCompra As Date = CDate(EjecutarSTRSQL_Scalar(myConn, lblInfo, _
                            '                                           " select FECHAPROXIMACOMPRA from jsvenCADIPposX WHERE TIPORAZON = '" & TipoRazon _
                            '                                           & "' AND DOCUMENTO = '" & Documento _
                            '                                           & "' AND CODIGO_PRODUCTO = '" & CodigoProducto _
                            '                                           & "' AND ID_EMP = '" & jytsistema.WorkID & "' ").ToString)

                            '    ft.mensajeCritico(" EL CLIENTE " & TipoRazon & "-" & Documento & " TIENE CONDICION BLOQUEADO, PARA LA MERCANCIA " & _
                            '                   DescripcionProductoBloqueado & ". PUEDE REALIZAR UNA NUEVA COMPRA EL DIA " & ft.muestracampofecha(FechaProximaCompra))

                            '    Return

                            'End If

                            ''''ojo ojo ojo 
                            ''''SE DEBE SUMAR LA CANTIDAD ANTERIOR DE ESTA FACTURA
                            Dim CantidadAdquirida As Integer = ft.DevuelveScalarEntero(myConn, " SELECT SUM(a.cantidad) " _
                                                                                     & " FROM jsvenrenpos a " _
                                                                                     & " LEFT JOIN jsmerctainv b ON (a.item = b.codart AND a.id_emp = b.id_emp) " _
                                                                                     & " WHERE " _
                                                                                     & " b.codjer = '" & CodigoProducto & "' AND " _
                                                                                     & " a.numfac = '" & numFacturaTemporal & "' AND " _
                                                                                     & " a.id_emp = '" & jytsistema.WorkID & "' " _
                                                                                     & " GROUP BY b.codjer")

                            CantidadAdquirida += ft.DevuelveScalarEntero(myConn, " SELECT SUM(a.cantidad) " _
                                                                                & " FROM jsvenrenpos a  " _
                                                                                & " LEFT JOIN jsmerctainv b ON (a.item = b.codart AND a.id_emp = b.id_emp) " _
                                                                                & " LEFT JOIN jsvenencpos c ON (a.numfac = c.numfac AND a.numserial = c.numserial AND a.id_emp = c.id_emp) " _
                                                                                & " WHERE " _
                                                                                & " b.codjer = '" & CodigoProducto & "' AND " _
                                                                                & " c.RIF = '" & RTrim(LTrim(txtRIFCI.Text)) & "' AND " _
                                                                                & " c.emision > '" & ft.FormatoFechaMySQL(DateAdd(DateInterval.Day, -1 * frecuenciaDias, jytsistema.sFechadeTrabajo)) & "' AND " _
                                                                                & " a.id_emp = '" & jytsistema.WorkID & "' GROUP BY b.codjer")



                            If CantidadMaximaDeVenta > 0 Then
                                If CantidadRenglon + CantidadAdquirida > CantidadMaximaDeVenta Then

                                    ft.mensajeCritico(" CUOTA ALCANZADA CLIENTE " & TipoRazon & "-" & Documento & ". PARA LA MERCANCIA " & _
                                                   qFoundAndSign(myConn, lblInfo, "jsmerctainv", aFld, aStr, "nomart") & ", PUEDE COMPRAR UNA CANTIDAD DE " & ft.muestraCampoEntero(CantidadMaximaDeVenta) & " UNIDADES ")

                                    Return

                                End If
                            End If

                        End If

                    End If


                    Dim MultiplicaTotal As Double = (1 - Descuento_articulo) * (1 - Descuento_cliente) * (1 - Descuento_oferta)

                    Dim TotalRenglon As Double = CantidadRenglon * PrecioUnitario * MultiplicaTotal
                    Dim NumeroRenglon As String = ft.autoCodigo(myConn, "RENGLON", "jsvenrenpos", "numfac.id_emp", numFacturaTemporal + "." + jytsistema.WorkID, 5)

                    ActualizarRenglonPV(myConn, lblInfo, numFacturaTemporal, numSerialFiscal, "0", 0, NumeroRenglon, _
                                       CodigoMercancia, _
                                       CodBarras, qFoundAndSign(myConn, lblInfo, "jsmerctainv", aFld, aStr, "nomart") & " " & CodTalla & " " & nomColor, _
                                       qFoundAndSign(myConn, lblInfo, "jsmerctainv", aFld, aStr, "IVA"), _
                                       UnidadVentaPOS, CantidadRenglon, PrecioUnitario, _
                                       PesoRenglon, "", DescuentoCliente, DescuentoArticulo, DescuentoOferta, TotalRenglon, TotalRenglon, "", 0)

                    ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
                    dtRenglones = ds.Tables(nTablaRenglones)

                    nPosicionRenglon = dtRenglones.Rows.Count - 1
                    AsignaMov(nPosicionRenglon, True)
                    txtBarras.BackColor = Color.PaleGreen

                Else
                    ft.mensajeAdvertencia(" Código de Barras Mercancía NO SE ENCUENTRA EN INVENTARIO ")
                End If
                EnfocarTextoTP(txtBarras)
                CantidadRenglon = 1

            End If
        End If

    End Sub

    Private Sub ValidaCantidadYOferta(MyConn As MySqlConnection, CodigoMercancia As String, NumeroFactura As String, _
                                      posicionActual As Long)
        '1. VERIFICA PARAMETRO PARA FACTURAR CON OFERTA AL ALCANZAR UN NUMERO DE UNIDADES ESPECIFICAS
        If CBool(ParametroPlus(MyConn, Gestion.iPuntosdeVentas, "POSPARAM34")) Then

            Dim nTarifa As String = ParametroPlus(MyConn, Gestion.iPuntosdeVentas, "POSPARAM35")
            Dim UnidadAdicional As String = ParametroPlus(MyConn, Gestion.iMercancías, "MERPARAM07")
            Dim cantidadRenglonesENUnidadAdicional As Double = ft.DevuelveScalarDoble(MyConn, " SELECT a.cantidad*b.equivale " _
                                                                        & " FROM (SELECT a.item, SUM( IF( b.uvalencia IS NULL, a.cantidad, a.cantidad / b.equivale )) cantidad,  " _
                                                                        & "         IF( b.unidad IS NULL, a.unidad, b.unidad) unidad, a.id_emp " _
                                                                        & "         FROM jsvenrenpos a " _
                                                                        & "         LEFT JOIN jsmerequmer b ON (a.item = b.codart AND a.unidad = b.uvalencia AND a.id_emp = b.id_emp) " _
                                                                        & "         WHERE " _
                                                                        & "         a.item = '" & CodigoMercancia & "'  AND " _
                                                                        & "         a.TIPO = 0 AND " _
                                                                        & "         a.numfac = '" & NumeroFactura & "' AND " _
                                                                        & "         a.id_emp = '" & jytsistema.WorkID & "') a " _
                                                                        & " LEFT JOIN (SELECT a.codart, a.uvalencia, a.equivale , a.id_emp " _
                                                                        & "            FROM jsmerequmer a " _
                                                                        & "            WHERE " _
                                                                        & "     	   a.codart = '" & CodigoMercancia & "' AND " _
                                                                        & "     	   a.id_emp = '" & jytsistema.WorkID & "' " _
                                                                        & "            UNION " _
                                                                        & "     	   SELECT a.codart, a.unidad, 1 equivale , a.id_emp " _
                                                                        & "            FROM jsmerctainv a " _
                                                                        & "            WHERE " _
                                                                        & "            a.codart = '" & CodigoMercancia & "' AND " _
                                                                        & "            a.id_emp = '" & jytsistema.WorkID & "') b ON (a.item = b.codart AND a.id_emp = b.id_emp) " _
                                                                        & " WHERE " _
                                                                        & " b.uvalencia = '" & UnidadAdicional & "' AND " _
                                                                        & " b.id_emp = '" & jytsistema.WorkID & "' ")


            For Each nRow As DataRow In dtRenglones.Rows

                With nRow

                    If .Item("ITEM") = CodigoMercancia And .Item("TIPO") = 0 Then

                        '//////////// DESCUENTOS GENERALES
                        Dim Equivalencia As Double = FuncionesMercancias.Equivalencia(MyConn, CodigoMercancia, .Item("UNIDAD"))
                        Dim desArt As Double = ft.DevuelveScalarDoble(MyConn, " select DESC_" & TarifaPrecios & " from jsmerctainv where codart = '" & CodigoMercancia & "' and id_emp = '" & jytsistema.WorkID & "' ")
                        Dim desCli As Double = ft.DevuelveScalarDoble(MyConn, " SELECT des_cli FROM jsvencatcli WHERE codcre = '0' AND codcli = '" & CodigoCliente & "' AND id_emp = '" & jytsistema.WorkID & "' ")
                        Dim desOfe As Double = 0 '///////////////// OJOJOJO DEBE SER ANALIZADO SEGUN LA IMPLEMENTACION AHORA = 0 PUES OFERTAS SE DAN EN EL MAYOR
                        Dim nPrecio As Double = ft.DevuelveScalarDoble(MyConn, " select precio_" & TarifaPrecios & " from jsmerctainv where codart = '" & CodigoMercancia & "' and id_emp = '" & jytsistema.WorkID & "' ") / IIf(Equivalencia = 0, 1, Equivalencia)

                        If cantidadRenglonesENUnidadAdicional >= 1 Then
                            '////// DESCUENTOS ADICIONALES POR COMPRA DE CAJAS
                            nPrecio = ft.DevuelveScalarDoble(MyConn, " select precio_" & nTarifa & " from jsmerctainv where codart = '" & CodigoMercancia & "' and id_emp = '" & jytsistema.WorkID & "' ") / IIf(Equivalencia = 0, 1, Equivalencia)
                            If nPrecio = 0.0 Then nPrecio = ft.DevuelveScalarDoble(MyConn, " select precio_" & TarifaPrecios & " from jsmerctainv where codart = '" & CodigoMercancia & "' and id_emp = '" & jytsistema.WorkID & "' ")
                            If CBool(ParametroPlus(MyConn, Gestion.iPuntosdeVentas, "POSPARAM36")) Then
                                desArt = 0
                                desCli = 0
                                desOfe = 0
                            End If

                        Else
                            '////DESCUENTOS POR PERFIL DE CAJERO
                            Select Case myPerfil.Descuento
                                Case 1
                                Case Else
                                    desArt = 0.0
                                    desCli = 0.0
                                    desOfe = 0.0
                            End Select

                        End If


                        '///// BLOQUEO DESCUENTOS POR PARAMETRO 
                        Dim DescuentoArticulo As Double = nPrecio * desArt / 100
                        Dim DescuentoCliente As Double = (nPrecio - DescuentoArticulo) * desCli / 100
                        If Not ParametroPlus(MyConn, Gestion.iPuntosdeVentas, "POSPARAM48") Then DescuentoCliente = 0.0
                        Dim DescuentoOferta As Double = (nPrecio - DescuentoArticulo - DescuentoCliente) * desOfe / 100

                        Dim nPrecioTotal As Double = nPrecio - DescuentoArticulo - DescuentoCliente - DescuentoOferta

                        ft.Ejecutar_strSQL(MyConn, " UPDATE jsvenrenpos set " _
                                                   & " des_art = " & desArt & ", " _
                                                   & " des_cli = " & desCli & ", " _
                                                   & " des_ofe = " & desOfe & ", " _
                                                   & " PRECIO = " & nPrecio & ", " _
                                                   & " TOTREN = " & nPrecio * .Item("CANTIDAD") & ", " _
                                                   & " TOTRENDES = " & nPrecioTotal * .Item("CANTIDAD") & " " _
                                                   & " WHERE " _
                                                   & " NUMFAC = '" & NumeroFactura & "' AND " _
                                                   & " NUMSERIAL = '" & .Item("NUMSERIAL") & "' AND " _
                                                   & " TIPO = " & .Item("TIPO") & " AND " _
                                                   & " RENGLON = '" & .Item("RENGLON") & "' AND " _
                                                   & " ITEM = '" & .Item("ITEM") & "' AND " _
                                                   & " ID_EMP = '" & jytsistema.WorkID & "' ")


                    End If

                End With

            Next

            AsignaMov(posicionActual, True)

        End If
        '2. EN CASO DE SER SI. DETERMINAR EL NUEVO PRECIO DE FACTURACION
        '3. COLOCAR DE NUEVO AL PRECIO INICIAL SI LA UNIDAD ES NORMAL
    End Sub


    '/// Datum3
    Private Sub CalculaTotales()


        If dtRenglones.Rows.Count > 0 And nPosicionRenglon >= 0 Then

            If myPerfil.Almacen <> "00001" Then
                ValidaCantidadYOferta(myConn, dtRenglones.Rows(nPosicionRenglon).Item("ITEM"), _
                                      numFacturaTemporal, nPosicionRenglon)
            End If

        End If

        ActualizarIVARenglonAlbaran(myConn, lblInfo, "jsvenivapos", "jsvenrenpos", "numfac", _
                                    numFacturaTemporal, jytsistema.sFechadeTrabajo, "totrendes", _
                                    numSerialFiscal)

        txtPesoTotal.Text = ft.muestraCampoCantidad(ft.DevuelveScalarDoble(myConn, " SELECT SUM(a.peso) " _
                    & " FROM jsvenrenpos a " _
                    & " WHERE " _
                    & " a.numfac = '" & numFacturaTemporal & "' AND  " _
                    & " a.id_emp = '" & jytsistema.WorkID & "' " _
                    & " GROUP BY a.numfac "))

        txtSubtotal.Text = ft.muestraCampoNumero(ft.DevuelveScalarDoble(myConn, " SELECT SUM(a.totrendes)  " _
                    & " FROM jsvenrenpos a " _
                    & " WHERE " _
                    & " a.numfac = '" & numFacturaTemporal & "' AND  " _
                    & " a.id_emp = '" & jytsistema.WorkID & "' " _
                    & " GROUP BY a.numfac "))

        txtDescuentos.Text = ft.muestraCampoNumero(ft.DevuelveScalarDoble(myConn, " SELECT SUM(a.descuento) " _
                    & " FROM jsvendespos a " _
                    & " WHERE " _
                    & " a.numfac = '" & numFacturaTemporal & "' AND " _
                    & " id_emp = '" & jytsistema.WorkID & "' group by a.numfac "))

        'txtIVA.Text = ft.muestraCampoNumero(ft.DevuelveScalarDoble(myConn, " SELECT SUM(a.impiva) " _
        '            & " FROM jsvenivapos a " _
        '            & " WHERE " _
        '            & " a.numfac = '" & numFacturaTemporal & "' AND " _
        '            & " a.id_emp = '" & jytsistema.WorkID & "' " _
        '            & " group by a.numfac "))

    End Sub

    Private Sub txtRIFCI_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtRIFCI.DoubleClick
        EnfocarTextoM(sender)
    End Sub

    Private Sub txtRIFCI_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtRIFCI.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Indique el número del documento de identidad ó el RIF ", Transportables.tipoMensaje.iInfo)
    End Sub

    Private Sub txtRIFCI_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtRIFCI.TextChanged

        If myConn.State = ConnectionState.Open Then

            Dim aCIF() As String = sender.text.Replace("_", "").Split("-")
            Dim txtCIF As String = ""

            If EsRIF(sender.text) Then
                txtCIF = aCIF(0) + "-" + aCIF(1) + "-" + aCIF(2)
            Else
                txtCIF = aCIF(0) + "-" + aCIF(1)
            End If

            Dim aFld() As String = {"rif", "id_emp"}
            Dim aStr() As String = {txtCIF, jytsistema.WorkID}

            dgDisponibilidad.Columns.Clear()
            ft.visualizarObjetos(False, grpDisponibilidad)

            If txtRIFCI.Text <> "" Then

                If qFound(myConn, lblInfo, "jsvencatcli", aFld, aStr) Then

                    CodigoCliente = qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "codcli")
                    txtNombre.Text = qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "nombre")
                    txtDireccion.Text = qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "dirfiscal")
                    txtTelefono.Text = qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "telef1")

                    CodigoVendedor = qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "vendedor")
                    NombreVendedor = ft.DevuelveScalarCadena(myConn, " select concat(apellidos, Nombres) from jsvencatven where tipo = '0' and codven = '" & CodigoVendedor & "' and id_emp = '" & jytsistema.WorkID & "' ")
                    If CodigoVendedor.Trim() = "" Then
                        CodigoVendedor = jytsistema.sUsuario
                        NombreVendedor = jytsistema.sNombreUsuario
                    End If

                    ft.habilitarObjetos(True, True, cmbPerfil)
                    ft.visualizarObjetos(True, grpDisponibilidad)
                    MostrarDisponibilidad(myConn, lblInfo, CodigoCliente, qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "rif"), _
                                ds, dgDisponibilidad)

                    Disponibilidad = ft.DevuelveScalarDoble(myConn, " select disponible from jsvencatcli where codcli = '" & CodigoCliente & "' and id_emp = '" & jytsistema.WorkID & "' ")
                    lblDisponibilidad.Text = "Disponible menos este Documento : " & ft.muestraCampoNumero(Disponibilidad)

                Else
                    CodigoCliente = "00000000"

                    CodigoVendedor = jytsistema.sUsuario
                    NombreVendedor = jytsistema.sNombreUsuario
                    ft.habilitarObjetos(False, True, cmbPerfil)

                    If qFound(myConn, lblInfo, "jsvencatclipv", aFld, aStr) Then
                        txtNombre.Text = qFoundAndSign(myConn, lblInfo, "jsvencatclipv", aFld, aStr, "nombre")
                        txtDireccion.Text = qFoundAndSign(myConn, lblInfo, "jsvencatclipv", aFld, aStr, "dirfiscal")
                        txtTelefono.Text = qFoundAndSign(myConn, lblInfo, "jsvencatclipv", aFld, aStr, "telef1")
                    Else
                        txtNombre.Text = ""
                        txtDireccion.Text = ""
                        txtTelefono.Text = ""
                    End If
                End If
            Else
                txtNombre.Text = ""
                txtDireccion.Text = ""
                txtTelefono.Text = ""
            End If

        End If

    End Sub

    Private Sub txtVendedorPiso_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtVendedorPiso.TextChanged
        Dim afld() As String = {"codven", "tipo", "id_emp"}
        Dim aStr() As String = {sender.text, TipoVendedor.iVendedorPiso, jytsistema.WorkID}
        txtVendedorPisoNombre.Text = qFoundAndSign(myConn, lblInfo, "jsvencatven", afld, aStr, "apellidos")
    End Sub


    Private Sub btnVendedor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVendedor.Click

        txtVendedorPiso.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codven codigo, apellidos descripcion from jsvencatven where tipo = '" & TipoVendedor.iVendedorPiso & "' and id_emp = '" & jytsistema.WorkID & "' ", "Vendedores de Piso", txtVendedorPiso.Text)

    End Sub

    Private Sub txtSubtotal_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSubtotal.TextChanged, _
    txtDescuentos.TextChanged, txtIVA.TextChanged, txtAbonado.TextChanged
        txtTotal.Text = ft.muestraCampoNumero(ValorNumero(txtSubtotal.Text) - ValorNumero(txtDescuentos.Text) + ValorNumero(txtIVA.Text))
        txtACancelar.Text = ft.muestraCampoNumero(ValorNumero(txtTotal.Text) - ValorNumero(txtAbonado.Text))
        lblACancelar.Text = txtACancelar.Text
        lblDisponibilidad.Text = "Disponible menos este Documento : " & ft.muestraCampoNumero(Disponibilidad - ValorNumero(txtACancelar.Text))
    End Sub
    Private Sub btnAgregarMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregarMovimiento.Click
        If IIf(EsRIF(txtRIFCI.Text.Trim), validarRif(txtRIFCI.Text.Trim), validarCI(txtRIFCI.Text.Trim.Split("-")(0) + "-" + txtRIFCI.Text.Trim.Split("-")(1).Trim)) Then
            If txtNombre.Text.Trim <> "" Then
                If cmbPerfil.Text = "" Then
                    ft.mensajeCritico("DEBE INDICAR UN PERFIL VALIDO")
                Else
                    Dim f As New jsGenRenglonesMovimientos
                    f.RIFCI = txtRIFCI.Text
                    f.Agregar(myConn, ds, dtRenglones, "PVE", numFacturaTemporal, jytsistema.sFechadeTrabajo, _
                              AlmacenSalida, TarifaPrecios, CodigoCliente)
                    nPosicionRenglon = f.Apuntador
                    AsignaMov(nPosicionRenglon, True)
                    CalculaTotales()
                    f = Nothing
                End If
            Else
                ft.mensajeCritico("DEBE INDICAR UN NOMBRE DE CLIENTE VALIDO")
            End If
        Else
            ft.mensajeCritico("DEBE INDICAR UNA CI/RIF DE CLIENTE VALIDO")
        End If
    End Sub
    Private Sub btnEditarMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditarMovimiento.Click
        If IIf(EsRIF(txtRIFCI.Text.Trim), validarRif(txtRIFCI.Text.Trim), validarCI(txtRIFCI.Text.Trim.Split("-")(0) + "-" + txtRIFCI.Text.Trim.Split("-")(1).Trim)) Then
            If txtNombre.Text.Trim <> "" Then
                If dtRenglones.Rows.Count > 0 Then
                    Dim g As New dtSupervisor
                    g.Cargar(myConn, ds)
                    If g.DialogResult = Windows.Forms.DialogResult.OK Then
                        Dim f As New jsGenRenglonesMovimientos
                        f.Apuntador = nPosicionRenglon
                        f.RIFCI = txtRIFCI.Text
                        f.Editar(myConn, ds, dtRenglones, "PVE", numFacturaTemporal, jytsistema.sFechadeTrabajo, _
                                 AlmacenSalida, TarifaPrecios, CodigoCliente)
                        nPosicionRenglon = f.Apuntador
                        AsignaMov(nPosicionRenglon, True)
                        CalculaTotales()
                        f = Nothing
                    End If
                    g.Dispose()
                    g = Nothing
                End If
            Else
                ft.mensajeCritico("DEBE INDICAR UN NOMBRE DE CLIENTE VALIDO")
            End If
        Else
            ft.mensajeCritico("DEBE INDICAR UNA CI/RIF DE CLIENTE VALIDO")
        End If
    End Sub
    Private Sub btnEliminarMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEliminarMovimiento.Click
        Dim f As New dtSupervisor
        f.Cargar(myConn, ds)
        If f.DialogResult = Windows.Forms.DialogResult.OK Then
            EliminaFila()
        End If
        f.Dispose()
        f = Nothing
    End Sub
    Private Sub EliminaFila()

        If nPosicionRenglon >= 0 Then
            Dim sRespuesta As Microsoft.VisualBasic.MsgBoxResult
            Dim aCamposDel() As String = {"numfac", "tipo", "renglon", "item", "id_emp"}
            Dim aStringsDel() As String = {numFacturaTemporal, "0", dtRenglones.Rows(nPosicionRenglon).Item("renglon"), _
                                           dtRenglones.Rows(nPosicionRenglon).Item("item"), jytsistema.WorkID}
            sRespuesta = MsgBox(" ¿ Esta Seguro que desea eliminar registro ?", MsgBoxStyle.YesNo, "Eliminar registro ... ")
            If sRespuesta = MsgBoxResult.Yes Then
                nPosicionRenglon = EliminarRegistros(myConn, lblInfo, ds, nTablaRenglones, "jsvenrenpos", strSQLMov, aCamposDel, aStringsDel, _
                                         nPosicionRenglon, True)
                AsignaMov(nPosicionRenglon, False)
                CalculaTotales()
            End If
        End If

    End Sub
    Private Sub dg_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles _
       dg.RowHeaderMouseClick, dg.CellMouseClick, dg.RegionChanged
        Me.BindingContext(ds, nTablaRenglones).Position = e.RowIndex
        nPosicionRenglon = e.RowIndex
        AsignaMov(e.RowIndex, False)
    End Sub

    Private Sub btnPrimerMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrimerMovimiento.Click
        Me.BindingContext(ds, nTablaRenglones).Position = 0
        AsignaMov(Me.BindingContext(ds, nTablaRenglones).Position, False)
    End Sub

    Private Sub btnAnteriorMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAnteriorMovimiento.Click
        Me.BindingContext(ds, nTablaRenglones).Position -= 1
        AsignaMov(Me.BindingContext(ds, nTablaRenglones).Position, False)
    End Sub

    Private Sub btnSiguienteMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSiguienteMovimiento.Click
        Me.BindingContext(ds, nTablaRenglones).Position += 1
        AsignaMov(Me.BindingContext(ds, nTablaRenglones).Position, False)
    End Sub

    Private Sub btnUltimoMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUltimoMovimiento.Click
        Me.BindingContext(ds, nTablaRenglones).Position = ds.Tables(nTablaRenglones).Rows.Count - 1
        AsignaMov(Me.BindingContext(ds, nTablaRenglones).Position, False)
    End Sub
    Private Sub btnDevolver_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDevolver.Click
        Dim ff As New dtSupervisor
        ff.Cargar(myConn, ds)
        If ff.DialogResult = Windows.Forms.DialogResult.OK Then
            If FacturaValida() Then
                Dim f As New jsPOSFacturaDevolucion
                f.Cargar(myConn, ds, jytsistema.WorkBox)
                If f.DialogResult = Windows.Forms.DialogResult.OK AndAlso f.NumeroFactura <> "" Then

                    Dim NumeroFacturaInternaAfectada As String = f.DocumentoInterno
                    Dim TipoImpresora As Integer = TipoImpresoraFiscal(myConn, jytsistema.WorkBox)

                    InsertEditVentasFormaPago(myConn, lblInfo, True, numFacturaTemporal, _
                                              numSerialFiscal, _
                                              "PVE", "EF", "", "", -1 * ValorNumero(txtTotal.Text), jytsistema.sFechadeTrabajo)

                    Dim NumNCReal As String = ""
                    Select Case TipoImpresora
                        Case 0
                            ft.mensajeInformativo("Imprime factura...")
                        Case 1 ' Factura Fiscal preimpresa
                            ft.mensajeInformativo("Imprimienso Factura fiscal")
                        Case 2, 5, 6, 7 ' Impresora Tipo Aclas (TFHKAIF.DLL)

                            '1. Crea encabezado
                            CrearEncabezado(f.NumeroFactura, EstatusFactura.eProcesada)

                            '2.- imprime factura
                            NumNCReal = Contador(myConn, lblInfo, Gestion.iPuntosdeVentas, "posnumncr", "02")
                            'While NumNCReal = ""
                            '    NumNCReal = NumeroUltimaNC(myConn, lblInfo, TipoImpresora)
                            'End While

                            Dim fechaFacturaAfectada As Date = CDate(f.FechaFacturaAfectada.Substring(0, 2) + "/" + _
                                                                f.FechaFacturaAfectada.Substring(2, 2) + "/" + f.FechaFacturaAfectada.Substring(4, 2))

                            condicionIVAEspecial = False
                            If tipoPersona(txtRIFCI.Text) = 1 And ft.DevuelveScalarDoble(myConn, "SELECT PORIVA " _
                                                                                          & " FROM jsvenivapos " _
                                                                                          & " WHERE " _
                                                                                          & " NUMFAC = '" & NumeroFacturaInternaAfectada & "' AND " _
                                                                                          & " TIPOIVA = 'A' AND " _
                                                                                          & " ID_EMP  ='" & jytsistema.WorkID & "'") = 10.0 Then condicionIVAEspecial = True


                            If TipoImpresora = 7 Then
                                Imprimir_NC_SRP812(myConn, lblInfo, numFacturaTemporal, txtNombre.Text, txtRIFCI.Text, txtDireccion.Text, _
                                                    txtTelefono.Text, jytsistema.sFechadeTrabajo, CodigoCliente, CStr(CondicionPago.iContado), _
                                                    fechaFacturaAfectada, CodigoVendedor, NombreVendedor, ValorNumero(txtTotal.Text), _
                                                    f.NumeroFactura, f.NumeroSerie, NumNCReal, condicionIVAEspecial)
                            Else
                                ImprimirNotaCreditoPP1F3(myConn, lblInfo, numFacturaTemporal, txtNombre.Text, txtRIFCI.Text, txtDireccion.Text, _
                                                    txtTelefono.Text, jytsistema.sFechadeTrabajo, CodigoCliente, CStr(CondicionPago.iContado), _
                                                    fechaFacturaAfectada, CodigoVendedor, NombreVendedor, ValorNumero(txtTotal.Text), _
                                                    f.NumeroFactura, f.NumeroSerie, NumNCReal, condicionIVAEspecial)
                            End If

                            '3.- Guardar NC
                            Dim UNC As String = NumeroUltimaNC(myConn, lblInfo, TipoImpresora)
                            GuardarFacturaNotaCredito_PuntoDeVenta(myConn, NumNCReal, _
                                              numSerialFiscal, _
                                              numFacturaTemporal, _
                                              numSerialFiscal, 1, _
                                              NumeroUltimaNC(myConn, lblInfo, TipoImpresora))

                            '4.- Incluye movimiento en caja
                            IncluirMovimientosNCEnCaja(myConn, NumNCReal, numSerialFiscal)

                            '5.- incluye movimiento en CXC
                            IncluyeMovimientoCXC(myConn, NumNCReal, 1, NumeroFacturaInternaAfectada)

                            '6.- Actualizar Movimiento en Inventario
                            ActualizarMovimientosInventario(myConn, NumNCReal, numSerialFiscal, False)

                            '7.- Incluye cliente pv 
                            IncluirClientePV(myConn, CodigoCliente)

                            '8.- Cerrar Ventana de fACTURA
                            ft.mensajeInformativo("NOTA CREDITO Terminada...")
                            f.Close()

                            '9.- Iniciar nueva factura
                            Iniciar_Apagado()

                        Case 3 ' Impresora Tipo Bematech (BEMAFI32.DLL)

                            '1. Crea encabezado
                            CrearEncabezado(f.NumeroFactura, EstatusFactura.eProcesada)

                            '2. Imprime Factura
                            ImprimirNCFiscalBematech(myConn, lblInfo, f.NumeroFactura, f.NumeroSerie, txtNombre.Text, txtRIFCI.Text, txtDireccion.Text, _
                                                txtTelefono.Text, jytsistema.sFechadeTrabajo, CodigoCliente, CondicionDePago, CodigoVendedor, _
                                                NombreVendedor)


                            NumNCReal = Contador(myConn, lblInfo, Gestion.iPuntosdeVentas, "posnumncr", "02")
                            While NumNCReal = ""
                                Retorno = Bematech_FI_LecturaXSerial()
                                If VerificaRetornoImpresora("", "", Retorno, "Lectura X por la Serial") Then _
                                    NumNCReal = NumeroUltimaNC(myConn, lblInfo, TipoImpresora)
                            End While

                            '3.- Guardar NC
                            GuardarFacturaNotaCredito_PuntoDeVenta(myConn, NumNCReal, numSerialFiscal, _
                                              numFacturaTemporal, _
                                              numSerialFiscal, 1, _
                                              NumeroUltimaNC(myConn, lblInfo, TipoImpresora))

                            '4.- Incluye movimiento en caja
                            IncluirMovimientosNCEnCaja(myConn, NumNCReal, numSerialFiscal)

                            '5.- incluye movimiento en CXC
                            IncluyeMovimientoCXC(myConn, NumNCReal, 1, NumeroFacturaInternaAfectada)

                            '6.- Actualizar Movimiento en Inventario
                            ActualizarMovimientosInventario(myConn, NumNCReal, numSerialFiscal, False)

                            '7.- Incluye cliente pv 
                            IncluirClientePV(myConn, CodigoCliente)

                            '8.- Cerrar Ventana de fACTURA
                            ft.mensajeInformativo("NOTA CREDITO Terminada...")
                            f.Close()

                            '9.- Iniciar nueva factura
                            Iniciar_Apagado()
                        Case 4 ''IMPRESORAS FISCALES PNP

                            '1. Crea encabezado
                            CrearEncabezado(f.NumeroFactura, EstatusFactura.eProcesada)

                            '2. Imprime Factura
                            ImprimirNotaCreditoPnP(myConn, lblInfo, numFacturaTemporal, f.NumeroFactura, f.NumeroSerie, f.FechaFacturaAfectada, _
                                                   f.HoraFacturaAfectada, txtNombre.Text, txtRIFCI.Text, txtDireccion.Text, _
                                                   jytsistema.sFechadeTrabajo, FechaVencimiento)

                            NumNCReal = Contador(myConn, lblInfo, Gestion.iPuntosdeVentas, "posnumncr", "02")
                            While NumNCReal = ""
                                NumNCReal = UltimaNCFiscalPnP(myConn, lblInfo)
                            End While

                            '3.- Guardar NC
                            GuardarFacturaNotaCredito_PuntoDeVenta(myConn, NumNCReal, numSerialFiscal, _
                                              numFacturaTemporal, _
                                              numSerialFiscal, 1, _
                                              NumeroUltimaNC(myConn, lblInfo, TipoImpresora))

                            '4.- Incluye movimiento en caja
                            IncluirMovimientosNCEnCaja(myConn, NumNCReal, numSerialFiscal)

                            '5.- incluye movimiento en CXC
                            IncluyeMovimientoCXC(myConn, NumNCReal, 1, NumeroFacturaInternaAfectada)

                            '6.- Actualizar Movimiento en Inventario
                            ActualizarMovimientosInventario(myConn, NumNCReal, numSerialFiscal, False)

                            '7.- Incluye cliente pv 
                            IncluirClientePV(myConn, CodigoCliente)

                            '8.- Cerrar Ventana de fACTURA
                            ft.mensajeInformativo("NOTA CREDITO Terminada...")
                            f.Close()

                            '9.- Iniciar nueva factura
                            Iniciar_Apagado()

                        Case Else

                    End Select
                End If
                f = Nothing
            End If
        End If
    End Sub
    Private Sub btnPagar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPagar.Click

        If FacturaValida() Then
            Dim f As New jsGenFormasPago
            f.CondicionPago = CondicionDePago
            f.TipoCredito = TipoCredito
            f.Vencimiento = jytsistema.sFechadeTrabajo

            Dim ClienteBloqueado As Integer = ft.DevuelveScalarEntero(myConn, " select estatus from jsvencatcli where codcli = '" & CodigoCliente & "' and id_emp = '" & jytsistema.WorkID & "' ")

            f.Cargar(myConn, "PVE", numFacturaTemporal, IIf(CodigoCliente <> "00000000" And CodigoCliente <> "", _
                            IIf(myPerfil.Credito, _
                            IIf(ClienteBloqueado = 0, True, False), False), False), ValorNumero(lblACancelar.Text), , , _
                            tipoPersona(txtRIFCI.Text))

            If f.DialogResult = Windows.Forms.DialogResult.OK Then
                ' 1. Crear Encabezado
                CondicionDePago = f.CondicionPago
                TipoCredito = f.TipoCredito
                FechaVencimiento = f.Vencimiento
                condicionIVAEspecial = f.condicionIVAPeriodoEspecial

                Dim TipoImpresora As Integer = TipoImpresoraFiscal(myConn, jytsistema.WorkBox)

                Dim NumFacturaReal As String = ""
                Dim TipoRazon As String = txtRIFCI.Text.Replace("_", "").Replace(" ", "").Split("-")(0)
                Dim Documento As String = txtRIFCI.Text.Replace("_", "").Replace(" ", "").Split("-")(1)
                Dim Identificador As String = txtRIFCI.Text.Replace("_", "").Replace(" ", "").Split("-")(2)
                If Identificador = "" Then Identificador = "0"

                Select Case TipoImpresora
                    Case 0 ' Imprime factura Gráfica de DATUM (FORMA LIBRE)
                        ' SE DIRIGE A LA IMPRESORA POR DEFECTO
                        '1. Encabezado
                        CrearEncabezado("", EstatusFactura.eProcesada)
                        NumFacturaReal = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMFAC", "08")
                        '
                        ' 3.- Guardar Factura 
                        GuardarFacturaNotaCredito_PuntoDeVenta(myConn, NumFacturaReal, _
                                                               numSerialFiscal, _
                                                               numFacturaTemporal, _
                                                               numSerialFiscal, 0)
                        ' 4.- Incluye movimiento en caja
                        IncluirMovimientosFCEnCaja(myConn, NumFacturaReal, numSerialFiscal)
                        ' 5.- Incluye movimiento en CXC
                        IncluyeMovimientoCXC(myConn, NumFacturaReal)
                        ' 6.- Actualizar movimientos inventario
                        'Dim ActualizaInventario As Boolean = CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM24"))
                        'If ActualizaInventario Then _
                        ActualizarMovimientosInventario(myConn, NumFacturaReal, numSerialFiscal, True)
                        ' 7.- Incluye cliente pv 
                        IncluirClientePV(myConn, CodigoCliente)

                        ActualizarMovimientosInventarioCADIP(myConn, NumFacturaReal, TipoRazon, Documento, Identificador, _
                            numSerialFiscal, jytsistema.sFechadeTrabajo)

                        ImprimirFacturaGraficaPOS(myConn, lblInfo, ds, NumFacturaReal)

                        ' 8.- Cerrar Ventana de pago
                        ft.mensajeInformativo("Factura Terminada...")
                        f.Close()
                        ' 9.- Inciar nueva factura (según parámetro ) 
                        Iniciar_Apagado()


                    Case 1 ' Factura Fiscal preimpresa
                        ft.mensajeInformativo("Imprimiendo Factura fiscal pre-impresa")
                    Case 2, 5, 6, 7 ' Impresora Tipo Aclas (TFHKAIF.DLL)
                        '1. Encabezado
                        CrearEncabezado("", EstatusFactura.eProcesada)

                        NumFacturaReal = Contador(myConn, lblInfo, Gestion.iPuntosdeVentas, "posnumfac", "01")
                        While NumFacturaReal = ""
                            NumFacturaReal = NumeroUltimaFactura(myConn, lblInfo, TipoImpresora)
                        End While
                        '2. Imprimir
                        ImprimirFacturaFiscalPP1F3(myConn, lblInfo, numFacturaTemporal, txtNombre.Text, CodigoCliente, _
                                                   txtRIFCI.Text, txtDireccion.Text, jytsistema.sFechadeTrabajo, _
                                                   CondicionDePago, FechaVencimiento, CodigoVendedor, NombreVendedor, _
                                                   NumFacturaReal, condicionIVAEspecial)

                        '3.- Guardar Factura 
                        GuardarFacturaNotaCredito_PuntoDeVenta(myConn, NumFacturaReal, numSerialFiscal, _
                                          numFacturaTemporal, numSerialFiscal, 0, _
                                          NumeroUltimaFactura(myConn, lblInfo, TipoImpresora))

                        ' 4.- Incluye movimiento en caja
                        IncluirMovimientosFCEnCaja(myConn, NumFacturaReal, numSerialFiscal)
                        ' 5.- Incluye movimiento en CXC
                        IncluyeMovimientoCXC(myConn, NumFacturaReal)
                        ' 6.- Actualizar movimientos inventario
                        'Dim ActualizaInventario As Boolean = CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM24"))
                        'If ActualizaInventario Then _
                        ActualizarMovimientosInventario(myConn, NumFacturaReal, numSerialFiscal, True)
                        ' 7.- Incluye cliente pv 
                        IncluirClientePV(myConn, CodigoCliente)
                        ' 8.- Cerrar Ventana de pago

                        ActualizarMovimientosInventarioCADIP(myConn, NumFacturaReal, TipoRazon, Documento, Identificador, _
                          numSerialFiscal, jytsistema.sFechadeTrabajo)


                        ft.mensajeInformativo("Factura Terminada...")
                        f.Close()
                        ' 9.- Inciar nueva factura (según parámetro ) 
                        Iniciar_Apagado()

                    Case 3 'Impresora Tipo Bematech (BEMAFI32.DLL)
                        CrearEncabezado("", EstatusFactura.eProcesada)
                        '2. Imprimir
                        ImprimirFacturaFiscalBematech(myConn, lblInfo, numFacturaTemporal, txtNombre.Text, txtRIFCI.Text, txtDireccion.Text, _
                                            txtTelefono.Text, jytsistema.sFechadeTrabajo, CodigoCliente, CondicionDePago, CodigoVendedor, _
                                            NombreVendedor)

                        While NumFacturaReal = ""
                            Retorno = Bematech_FI_LecturaXSerial()
                            If VerificaRetornoImpresora("", "", Retorno, "Lectura X por la Serial") Then _
                                NumFacturaReal = NumeroUltimaFactura(myConn, lblInfo, TipoImpresora)
                        End While

                        ' 3.- Guardar Factura 
                        GuardarFacturaNotaCredito_PuntoDeVenta(myConn, NumFacturaReal, numSerialFiscal, _
                                          numFacturaTemporal, _
                                          numSerialFiscal, 0, _
                                          NumeroUltimaFactura(myConn, lblInfo, TipoImpresora))

                        ' 4.- Incluye movimiento en caja
                        IncluirMovimientosFCEnCaja(myConn, NumFacturaReal, numSerialFiscal)

                        ' 5.- Incluye movimiento en CXC
                        IncluyeMovimientoCXC(myConn, NumFacturaReal)

                        ' 6.- Actualizar movimientos inventario
                        'Dim ActualizaInventario As Boolean = CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM24"))
                        'If ActualizaInventario Then _
                        ActualizarMovimientosInventario(myConn, NumFacturaReal, numSerialFiscal, True)

                        ' 7.- Incluye cliente pv 
                        IncluirClientePV(myConn, CodigoCliente)

                        ' 8.- cerrar ventana de pago 
                        ActualizarMovimientosInventarioCADIP(myConn, NumFacturaReal, TipoRazon, Documento, Identificador, _
                               numSerialFiscal, jytsistema.sFechadeTrabajo)

                        ft.mensajeInformativo("Factura Terminada...")
                        f.Close()

                        ' 9.- Inciar nueva factura (según parámetro ) 
                        Iniciar_Apagado()

                    Case 4 'Impresora Epson/PnP

                        CrearEncabezado("", EstatusFactura.eProcesada)

                        ImprimirFacturaFiscalPnP(myConn, lblInfo, numFacturaTemporal, txtNombre.Text, CodigoCliente, txtRIFCI.Text, txtDireccion.Text, _
                                           jytsistema.sFechadeTrabajo, CondicionDePago, FechaVencimiento, CodigoVendedor, _
                                            NombreVendedor)

                        Dim ultimaFac As String = UltimaFACTURAImpresoraFiscal(myConn, lblInfo, jytsistema.WorkBox)

                        Dim ValorUltimaFactura As String = "0"
                        If ultimaFac <> "" Then ValorUltimaFactura = ultimaFac

                        NumFacturaReal = Contador(myConn, lblInfo, Gestion.iPuntosdeVentas, "posnumfac", "01")
                        While NumFacturaReal = ""
                            NumFacturaReal = UltimaFCFiscalPnP(myConn, lblInfo)
                        End While

                        ' 3.- Guardar Factura 
                        GuardarFacturaNotaCredito_PuntoDeVenta(myConn, NumFacturaReal, numSerialFiscal, _
                                          numFacturaTemporal, _
                                          numSerialFiscal, 0, _
                                          NumeroUltimaFactura(myConn, lblInfo, TipoImpresora))

                        ' 4.- Incluye movimiento en caja
                        IncluirMovimientosFCEnCaja(myConn, NumFacturaReal, numSerialFiscal)
                        ' 5.- Incluye movimiento en CXC
                        IncluyeMovimientoCXC(myConn, NumFacturaReal)
                        ' 6.- Actualizar movimientos inventario
                        'Dim ActualizaInventario As Boolean = CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM24"))
                        'If ActualizaInventario Then _
                        ActualizarMovimientosInventario(myConn, NumFacturaReal, numSerialFiscal, True)
                        ' 7.- Incluye cliente pv 
                        IncluirClientePV(myConn, CodigoCliente)
                        ' 8.- cerrar ventana de pago 
                        ft.mensajeInformativo("Factura Terminada...")
                        f.Close()
                        ' 9.- Inciar nueva factura (según parámetro ) 
                        Iniciar_Apagado()

                    Case Else

                End Select

            End If
            f = Nothing
        End If

    End Sub
    Private Sub CrearEncabezado(ByVal NumeroReferencia As String, ByVal EstatusFC As EstatusFactura)

        Dim afld() As String = {"numfac", "id_emp"}
        Dim aStr() As String = {numFacturaTemporal, jytsistema.WorkID}
        If Not qFound(myConn, lblInfo, "jsvenencpos", afld, aStr) Then

            InsertarModificarPOSEncabezadoPuntoDeVenta(myConn, lblInfo, True, numFacturaTemporal, numSerialFiscal, 0, jytsistema.sFechadeTrabajo, CodigoCliente, _
                    RTrim(LTrim(txtNombre.Text)), RTrim(LTrim(txtRIFCI.Text)), "", txtDireccion.Text, AlmacenSalida, jytsistema.sFechadeTrabajo, _
                    NumeroReferencia, numFacturaTemporal, txtVendedorPiso.Text, dtRenglones.Rows.Count, 0.0#, ValorCantidad(txtPesoTotal.Text), _
                    ValorNumero(txtSubtotal.Text), 0.0#, ValorNumero(txtDescuentos.Text), 0.0#, _
                    ValorNumero(txtIVA.Text), ValorNumero(txtTotal.Text), CondicionDePago, TipoCredito, 0.0, "", "", 0.0, _
                    "", "", 0.0, "", "", 0.0, "", "", 0.0, "", jytsistema.sFechadeTrabajo, _
                     EstatusFC, TarifaPrecios, jytsistema.WorkBox, CodigoVendedor, 0)

        End If

    End Sub
    Private Sub IncluirClientePV(ByVal MyConn As MySqlConnection, ByVal CodigoCliente As String)
        'incluye cliente si es contado
        Dim sCIF As String = ""
        Dim TipoRazon As String = txtRIFCI.Text.Replace("_", "").Replace(" ", "").Split("-")(0)
        Dim Documento As String = txtRIFCI.Text.Replace("_", "").Replace(" ", "").Split("-")(1)
        Dim Identificador As String = txtRIFCI.Text.Replace("_", "").Replace(" ", "").Split("-")(2)

        If EsRIF(txtRIFCI.Text) Then
            sCIF = TipoRazon + "-" + Documento + "-" + Identificador
        Else
            sCIF = TipoRazon + "-" + Documento
        End If

        Dim IncluyeCliente As Boolean = False

        Dim aFld() As String = {"rif", "id_emp"}
        Dim aStr() As String = {sCIF, jytsistema.WorkID}
        If CodigoCliente = "00000000" Then
            If Not qFound(MyConn, lblInfo, "jsvencatclipv", aFld, aStr) Then IncluyeCliente = True
            InsertarModificarPOSClientePV(MyConn, lblInfo, IncluyeCliente, "00000000", txtNombre.Text, "", "", sCIF, "", _
                    "", "", txtDireccion.Text, txtTelefono.Text, "", jytsistema.sFechadeTrabajo, 1)
        End If

    End Sub
    Private Function FacturaValida() As Boolean

        If Trim(txtRIFCI.Text) = "" Then
            ft.mensajeAdvertencia(" CI o RIF no válido. Verifique por favor...")
            EnfocarTextoM(txtRIFCI)
            Exit Function
        Else
            If Not IIf(EsRIF(txtRIFCI.Text.Trim), validarRif(txtRIFCI.Text.Trim), validarCI(txtRIFCI.Text.Trim.Split("-")(0) + "-" + txtRIFCI.Text.Trim.Split("-")(1).Trim)) Then
                ft.mensajeAdvertencia(" CI o RIF no válido. Debe indicarlo de la forma V-11111111 ...")
                EnfocarTextoM(txtRIFCI)
                Exit Function
            End If
        End If

        'If ParametroPlus(MyConn,  Gestion.iPuntosdeVentas, "POSPARAM02") = 1 AndAlso txtNombre.Text = "" Then
        If Trim(txtNombre.Text) = "" Then
            ft.mensajeCritico(" DEBE INDICAR UN NOMBRE DE CLIENTE VALIDO ...")
            txtNombre.Focus()
            Exit Function
        End If

        If ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM04") = 1 AndAlso txtTelefono.Text = "" Then
            ft.mensajeAdvertencia(" DEBE INDICAR UN NUMERO DE TELEFONO VALIDO ...")
            txtTelefono.Focus()
            Exit Function
        End If

        If ValorEntero(itemsrenglon.Text) = 0 Then
            ft.mensajeAdvertencia(" DEBE INCLUIR POR LO MENOS UN ITEM ...")
            Exit Function
        End If

        FacturaValida = True

    End Function

    Private Sub IncluirMovimientosFCEnCaja(ByVal MyConn As MySqlConnection, ByVal NumeroFactura As String, ByVal NumeroSerialFiscal As String)

        If CondicionDePago = CondicionPago.iContado Then
            Dim dtFP As DataTable
            Dim nTablaFP As String = "tblFP"

            ds = DataSetRequery(ds, " select * from jsvenforpag where numfac = '" & NumeroFactura & "' AND " _
                                & " id_emp = '" & jytsistema.WorkID & "' ", MyConn, nTablaFP, lblInfo)

            dtFP = ds.Tables(nTablaFP)
            If dtFP.Rows.Count > 0 Then
                Dim iCont As Integer
                For iCont = 0 To dtFP.Rows.Count - 1
                    With dtFP.Rows(iCont)
                        InsertarModificarPOSWork(MyConn, lblInfo, True, jytsistema.WorkBox, jytsistema.sFechadeTrabajo, _
                                                 "PVE", "EN", NumeroFactura, NumeroSerialFiscal, .Item("formapag"), _
                                                 .Item("numpag"), .Item("nompag"), .Item("importe"), _
                                                 CDate(.Item("vence").ToString), 1, jytsistema.sUsuario)
                    End With
                Next
            End If
            dtFP.Dispose()
            dtFP = Nothing
        Else
            InsertarModificarPOSWork(MyConn, lblInfo, True, jytsistema.WorkBox, jytsistema.sFechadeTrabajo, "PVE", "EN", _
                                     NumeroFactura, NumeroSerialFiscal, "CR", "", _
                                     "", ValorNumero(txtACancelar.Text), FechaVencimiento, 1, jytsistema.sUsuario)
        End If


    End Sub

    Private Sub IncluirMovimientosNCEnCaja(ByVal MyConn As MySqlConnection, ByVal NumeroNC As String, ByVal NumeroSerialFiscal As String)

        InsertarModificarPOSWork(MyConn, lblInfo, True, jytsistema.WorkBox, jytsistema.sFechadeTrabajo, "PVE", "SA", NumeroNC, _
                                 NumeroSerialFiscal, "EF", "", _
                    "", ValorNumero(txtACancelar.Text), FechaVencimiento, 1, jytsistema.sUsuario)

    End Sub

    Private Sub IncluyeMovimientoCXC(ByVal MyConn As MySqlConnection, ByVal NumeroControl As String, Optional ByVal Devolucion As Integer = 0, _
                                     Optional ByVal DocumentoInterno As String = "")
        If Devolucion = 0 Then
            If CodigoCliente <> "00000000" And CodigoCliente <> "" Then
                If CondicionDePago = CondicionPago.iContado Then
                Else
                    Dim Insertar As Boolean = False
                    Dim aFld() As String = {"codcli", "emision", "tipomov", "nummov", "origen", "ejercicio", "id_emp"}
                    Dim aStr() As String = {CodigoCliente, ft.FormatoFechaMySQL(jytsistema.sFechadeTrabajo), "FC", NumeroControl, "PVE", jytsistema.WorkExercise, jytsistema.WorkID}

                    If Not qFound(MyConn, lblInfo, "jsventracob", aFld, aStr) Then Insertar = True

                    InsertEditVENTASCXC(MyConn, lblInfo, Insertar, CodigoCliente, "FC", NumeroControl, jytsistema.sFechadeTrabajo, Format(Now(), "00:00:00"), _
                        FechaVencimiento, "", "FC : " & NumeroControl, ValorNumero(txtACancelar.Text), ValorNumero(txtIVA.Text), _
                        "", "", "", "", "", "PVE", NumeroControl, "0", "", jytsistema.sFechadeTrabajo, "", _
                        "", "", 0.0#, 0.0#, "", "", "", "", CodigoVendedor, CodigoVendedor, "0", "0", "")

                End If
            End If
        Else

            If CodigoCliente <> "00000000" And CodigoCliente <> "" Then

                If DocumentoInterno <> "" Then

                    Dim CondicionPagoDocumentoInterno As Integer = ft.DevuelveScalarEntero(MyConn, " select condpag from jsvenencpos where numfac = '" & DocumentoInterno & "' and id_emp = '" & jytsistema.WorkID & "' ")

                    If CondicionPagoDocumentoInterno = 0 Then
                        Dim Insertar As Boolean = False
                        Dim aFld() As String = {"codcli", "emision", "tipomov", "nummov", "origen", "ejercicio", "id_emp"}
                        Dim aStr() As String = {CodigoCliente, ft.FormatoFechaMySQL(jytsistema.sFechadeTrabajo), "NC", NumeroControl, "PVE", jytsistema.WorkExercise, jytsistema.WorkID}
                        If Not qFound(MyConn, lblInfo, "jsventracob", aFld, aStr) Then Insertar = True

                        InsertEditVENTASCXC(MyConn, lblInfo, Insertar, CodigoCliente, "NC", DocumentoInterno, jytsistema.sFechadeTrabajo, Format(Now(), "00:00:00"), _
                            FechaVencimiento, "", "NC : " & DocumentoInterno, -1 * ValorNumero(txtACancelar.Text), -1 * ValorNumero(txtIVA.Text), _
                            "", "", "", "", "", "PVE", DocumentoInterno, "0", "", jytsistema.sFechadeTrabajo, "", _
                            "", "", 0.0#, 0.0#, "", "", "", "", CodigoVendedor, CodigoVendedor, "0", "1", "")

                    End If

                End If

            End If

        End If
    End Sub

    Private Sub ActualizarMovimientosInventarioCADIP(ByVal MyConn As MySqlConnection, ByVal NumeroFactura As String, _
                                                ByVal TipoRazon As String, Documento As String, Identificador As String, _
                                                NumeroSerialFiscal As String, FechaFactura As Date)


        Dim numeritoFactura As Long = ValorEnteroLargo(NumeroFactura)

        ft.Ejecutar_strSQL(MyConn, " delete from jsvenCADIPpos " _
                       & " where " _
                       & " FACTURA = '" & numeritoFactura & "' AND " _
                       & " TIPORAZON = '" & TipoRazon & "' AND " _
                       & " DOCUMENTO = '" & Documento & "' AND " _
                       & " IDENTIFICADOR = '" & Identificador & "' AND " _
                       & " id_emp = '" & jytsistema.WorkID & "' ")

        ds = DataSetRequery(ds, "select * from jsvenrenpos " _
            & " where " _
            & " numfac  = '" & NumeroFactura & "' and " _
            & " ejercicio = '" & jytsistema.WorkExercise & "' and " _
            & " id_emp = '" & jytsistema.WorkID & "' order by RENGLON ", MyConn, nTablaRenglones, lblInfo)

        dtRenglones = ds.Tables(nTablaRenglones)
        If dtRenglones.Rows.Count > 0 Then
            Dim rCont As Integer
            For rCont = 0 To dtRenglones.Rows.Count - 1
                With dtRenglones.Rows(rCont)
                    If Mid(.Item("ITEM"), 1, 1) <> "$" Then

                        Dim MercanciaRegulada As Boolean = ft.DevuelveScalarBooleano(MyConn, " select REGULADO from jsmerctainv where codart = '" & .Item("item") & "' and id_emp = '" & jytsistema.WorkID & "' ")

                        If MercanciaRegulada Then

                            Dim CodigoProducto As String = ft.DevuelveScalarCadena(MyConn, " select CODJER from jsmerctainv where codart = '" & .Item("item") & "' and id_emp = '" & jytsistema.WorkID & "' ").ToString.Replace(".", "")
                            If CodigoProducto.Trim() = "" Then CodigoProducto = "0"

                            If CInt(CodigoProducto) <> 0 Then
                                InsertarModificarCADIP(MyConn, True, TipoRazon, Documento, Identificador, _
                                    .Item("cantidad"), CodigoProducto, FechaFactura, ft.FormatoHora(Now()), _
                                    numeritoFactura, 0, jytsistema.sFechadeTrabajo)
                            End If


                        End If

                    End If

                End With

            Next
        End If

    End Sub


    Private Sub ActualizarMovimientosInventario(ByVal MyConn As MySqlConnection, ByVal NumeroFactura As String, _
                                                ByVal NumeroSerialFiscal As String, _
                                                ByVal Factura_Devolucion As Boolean)

        CalculaDescuentosEnRenglones(MyConn, NumeroFactura, NumeroSerialFiscal, ValorNumero(txtDescuentos.Text))

        ft.Ejecutar_strSQL(MyConn, " delete from jsmertramer where " _
                           & " numorg = '" & NumeroFactura & "' and " _
                           & " origen = 'PVE' and " _
                           & " id_emp = '" & jytsistema.WorkID & "' ")

        ds = DataSetRequery(ds, "select * from jsvenrenpos " _
            & " where " _
            & " numfac  = '" & NumeroFactura & "' and " _
            & " id_emp = '" & jytsistema.WorkID & "' order by RENGLON ", MyConn, nTablaRenglones, lblInfo)

        dtRenglones = ds.Tables(nTablaRenglones)
        If dtRenglones.Rows.Count > 0 Then
            Dim rCont As Integer
            For rCont = 0 To dtRenglones.Rows.Count - 1
                With dtRenglones.Rows(rCont)
                    If Mid(.Item("ITEM"), 1, 1) <> "$" Then
                        If Factura_Devolucion Then ft.Ejecutar_strSQL(MyConn, " update jsmerctainv SET " _
                            & " fecultventa = '" & ft.FormatoFechaMySQL(jytsistema.sFechadeTrabajo) & "', ultimocliente = '" & CodigoCliente & "', montoultimaventa = " & .Item("precio") & "  " _
                            & " where " _
                            & " codart = '" & .Item("item") & "' and " _
                            & " id_emp = '" & jytsistema.WorkID & "' ")

                        Dim nEquivale As Double = Equivalencia(MyConn, .Item("item"), .Item("unidad"))
                        Dim nCosto As Double = UltimoCostoAFecha(MyConn, .Item("item"), jytsistema.sFechadeTrabajo)
                        Dim Costototal As Double = nCosto * .Item("cantidad") / IIf(nEquivale = 0, 1, nEquivale)

                        InsertEditMERCASMovimientoInventario(MyConn, lblInfo, True, .Item("item"), jytsistema.sFechadeTrabajo, IIf(Factura_Devolucion, "SA", "EN"), _
                            NumeroFactura, .Item("unidad"), .Item("cantidad"), .Item("peso"), Costototal, Costototal, "PVE", NumeroFactura, _
                            IIf(IsDBNull(.Item("lote")), "", .Item("LOTE")), CodigoCliente, .Item("totren"), .Item("totrendes"), 0.0, .Item("totren") - .Item("totrendes"), _
                            CodigoVendedor, AlmacenSalida, .Item("renglon") + .Item("estatus"), jytsistema.sFechadeTrabajo)

                        ActualizarExistenciasPlus(MyConn, .Item("ITEM"), AlmacenSalida)

                    End If
                End With

            Next
        End If

    End Sub
    Private Sub CalculaDescuentosEnRenglones(ByVal MyConn As MySqlConnection, ByVal NumeroFactura As String, ByVal NumeroSerialFiscal As String, ByVal TotalDescuento As Double)

        Dim TotalMercanciasConDescuento As Double = ft.DevuelveScalarDoble(MyConn, " select sum(totren) from " _
                                                & " jsvenrenpos " _
                                                & " where " _
                                                & " numfac = '" & NumeroFactura & "' and " _
                                                & " tipo = 0 and " _
                                                & " estatus = 0 and " _
                                                & " id_emp = '" & jytsistema.WorkID & "' ")

        ft.Ejecutar_strSQL(MyConn, " update jsvenrenpos set totrendes = totren - if( " & TotalMercanciasConDescuento * TotalDescuento & " <= 0, 0, round(totren/" & TotalMercanciasConDescuento * TotalDescuento & ",2) ) " _
                    & " where " _
                    & " numfac = '" & NumeroFactura & "' and " _
                    & " tipo = 0 and " _
                    & " estatus = 0 and " _
                    & " id_emp = '" & jytsistema.WorkID & "' ")

    End Sub


    Private Sub btnMercancias_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMercancias.Click
        Dim f As New jsMerArcListaCostosPreciosNormal
        f.Cargar(myConn, TipoListaPrecios.Precios_IVA, AlmacenSalida, , ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM11"))
        f = Nothing
    End Sub

    Private Sub btnReporteX_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReporteX.Click
        Dim TipoImpresora As Integer = TipoImpresoraFiscal(myConn, jytsistema.WorkBox)
        EsperateUnPoquito(myConn)
        Select Case TipoImpresora
            Case 0
                ft.mensajeInformativo("Imprime factura...")
            Case 1 ' Factura Fiscal preimpresa
                ft.mensajeInformativo("Imprimienso Factura fiscal")
            Case 2, 5, 6, 7 ' Impresora Tipo Aclas (TFHKAIF.DLL)
                bRet = IB.abrirPuerto(PuertoImpresoraFiscal(myConn, lblInfo, jytsistema.WorkBox))
                If bRet Then
                    bRet = IB.ReporteXFiscal()
                    IB.cerrarPuerto()
                End If
            Case 3 ' Impresora Tipo Bematech (BEMAFI32.DLL)
                ReporteXFiscalBematech()
            Case 4 'Impresora Fiscal Epson/PnP
                ReporteXFiscalPnP(myConn, lblInfo)
        End Select

        Dim f As New jsPOSRepParametros
        f.Cargar(TipoCargaFormulario.iShowDialog, ReportePuntoDeVenta.cReporteX, "REPORTE X", jytsistema.WorkBox)
        f = Nothing
    End Sub

    Private Sub btnReporteZ_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReporteZ.Click
        Dim f As New dtSupervisor
        f.Cargar(myConn, ds)
        If f.DialogResult = Windows.Forms.DialogResult.OK Then
            If Not FacturasPendientes(myConn, lblInfo, jytsistema.WorkBox, jytsistema.sUsuario) Then
                Dim g As New jsPOSProCierre
                g.Cargar(myConn, True)
                g = Nothing
            Else
                ft.mensajeCritico(" EXISTEN FACTURAS PENDIENTES EN CONFORMACIÓN. CIERRELAS Y E INTENTE DE NUEVO ... ")
            End If
        End If
        f.Close()
    End Sub

    Private Sub btnSubir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSubir.Click
        '1.- Crear Encabezado factura por confirmar
        If txtNombre.Text <> "" Then
            CrearEncabezado("", EstatusFactura.ePorConfirmar)
            '2.- Incluir cliente de punto de venta
            IncluirClientePV(myConn, CodigoCliente)
            '3.- Inciar nueva factura (según parámetro) 
            Iniciar_Apagado()
            'IniciarFactura()
        Else
            ft.mensajeCritico("DEBE INDICAR UN NOMBRE PARA ESTE CLIENTE...")
        End If
    End Sub

    Private Sub btnBajar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBajar.Click
        Dim f As New jsPOSFacturasXConformar
        f.Cargar(myConn, TipoCargaFormulario.iShowDialog)
        If f.Seleccionado <> "" Then
            ft.visualizarObjetos(True, grpFactura)
            IniciarFactura(f.Seleccionado)
            ApagarFactura()
        End If
        f.Dispose()
        f = Nothing
    End Sub

    Private Sub btnCliente_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCliente.Click
        Dim dtClientes As DataTable
        Dim tblClientes As String = "tblClientes"

        ds = DataSetRequery(ds, " (select a.rif codigo, a.nombre descripcion " _
                            & " from jsvencatcli a where a.id_emp = '" & jytsistema.WorkID & "') " _
                            & " union (select b.rif codigo , b.nombre descripcion  " _
                            & " from jsvencatclipv b where b.id_emp = '" & jytsistema.WorkID & "' ) order by 1  ", myConn, tblClientes, lblInfo)

        dtClientes = ds.Tables(tblClientes)
        If dtClientes.Rows.Count > 0 Then
            Dim aCam() As String = {"codigo", "descripcion", ""}
            Dim aStr() As String = {"RIF/CI Cliente", "Nombre o razón social", ""}
            Dim aWth() As Integer = {100, 350, 100}
            Dim f As New frmBuscar
            f.Buscar(dtClientes, aCam, aStr, aWth, 0, "Clientes ")

            If f.DialogResult = Windows.Forms.DialogResult.OK Then
                txtRIFCI.Text = dtClientes.Rows(f.Apuntador).Item("codigo")
            Else
                txtRIFCI.Text = ""
            End If

        End If

    End Sub

    Private Sub txtNombre_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtNombre.GotFocus, txtNombre.Click
        ft.mensajeEtiqueta(lblInfo, "Indique el nombre o razón social del cliente...", Transportables.tipoMensaje.iInfo)
        ft.enfocarTexto(sender)
    End Sub

    Private Sub txtDireccion_GotFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtDireccion.GotFocus, txtDireccion.Click
        ft.mensajeEtiqueta(lblInfo, "Indique la dirección del cliente...", Transportables.tipoMensaje.iInfo)
        ft.enfocarTexto(sender)
    End Sub

    Private Sub txtTelefono_GotFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTelefono.GotFocus, txtTelefono.Click
        ft.mensajeEtiqueta(lblInfo, "Indique el teléfono del cliente...", Transportables.tipoMensaje.iInfo)
        ft.enfocarTexto(sender)
    End Sub

    Private Sub btnReimprimir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReImprimirFacturas.Click
        '
        Select Case TipoImpresoraFiscal(myConn, jytsistema.WorkBox)
            Case 2, 5, 6
                ReimprimirUltimoDocumentoFiscal(myConn, lblInfo, AclasBixolon.tipoDocumentoFiscal.Factura)
            Case 7
                ReimprimirUltimoDocumentoFiscal(myConn, lblInfo, AclasBixolon.tipoDocumentoFiscal.FC_SRP812)
            Case Else
                ft.mensajeCritico("ESTE MODELO DE IMPRESORA FISCAL NO PUEDE REIMPRIMIR DOCUMENTO")
        End Select


    End Sub
    Private Sub btnReimprimirNotasCredito_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReimprimirNotasDeCredito.Click
        '
        Select Case TipoImpresoraFiscal(myConn, jytsistema.WorkBox)
            Case 2, 5, 6
                ReimprimirUltimoDocumentoFiscal(myConn, lblInfo, AclasBixolon.tipoDocumentoFiscal.NotaCredito)
            Case 7
                ReimprimirUltimoDocumentoFiscal(myConn, lblInfo, AclasBixolon.tipoDocumentoFiscal.NC_SRP812)
            Case Else
                ft.mensajeCritico("ESTE MODELO DE IMPRESORA FISCAL NO PUEDE REIMPRIMIR DOCUMENTO")
        End Select


    End Sub
    Private Sub btnReimprimirNoFiscal_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReimprimirDocumentoNF.Click
        '
        Select Case TipoImpresoraFiscal(myConn, jytsistema.WorkBox)
            Case 2, 5, 6
                ReimprimirUltimoDocumentoFiscal(myConn, lblInfo, AclasBixolon.tipoDocumentoFiscal.Nofiscal)
            Case 7
                ReimprimirUltimoDocumentoFiscal(myConn, lblInfo, AclasBixolon.tipoDocumentoFiscal.NF_SRP812)
            Case Else
                ft.mensajeCritico("ESTE MODELO DE IMPRESORA FISCAL NO PUEDE REIMPRIMIR DOCUMENTO")
        End Select


    End Sub

    Private Sub btnConfigurar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnConfigurar.Click


        Dim TipoImpresora As Integer = TipoImpresoraFiscal(myConn, jytsistema.WorkBox)

        Select Case TipoImpresora
            Case 0 'Factura Normal
            Case 1 'Factura Fiscal preimpresa
            Case 2, 5, 6, 7 'Impresora Tipo Aclas (TFHKAIF.DLL)
                Dim f As New jsPOSProConfigurarAclas
                f.Cargar(myConn)
                f = Nothing
            Case 3 'Impresora Tipo Bematech (BEMAFI32.DLL)
            Case 4 'Impresora tipo Epson/PnP 
                MsgBox(UltimaFCFiscalPnP(myConn, lblInfo))
            Case Else

        End Select

    End Sub

    Private Sub btnRecambios_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRecambios.Click
        Dim ff As New dtSupervisor
        ff.Cargar(myConn, ds)
        If ff.DialogResult = Windows.Forms.DialogResult.OK Then
            Dim gg As New jsPOSRecambioMercancia
            gg.Cargar(myConn, ds, jytsistema.WorkBox, jytsistema.sUsuario, myPerfil.Almacen)
            gg.Dispose()
            gg = Nothing
        End If
        ff.Dispose()
        ff = Nothing
    End Sub

    Private Sub dg_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles dg.KeyUp
        Select Case e.KeyCode
            Case Keys.Down
                Me.BindingContext(ds, nTablaRenglones).Position += 1
                nPosicionRenglon = Me.BindingContext(ds, nTablaRenglones).Position
                AsignaMov(nPosicionRenglon, False)
            Case Keys.Up
                Me.BindingContext(ds, nTablaRenglones).Position -= 1
                nPosicionRenglon = Me.BindingContext(ds, nTablaRenglones).Position
                AsignaMov(nPosicionRenglon, False)
        End Select
    End Sub


    Private Sub cmbPerfil_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbPerfil.SelectedIndexChanged

        If cmbPerfil.Items.Count > 0 Then

            myPerfil = TraerPerfil(myConn, ds, lblInfo, cmbPerfil.Text.Split("|")(0))

            If myPerfil.TarifaA Then
                TarifaPrecios = "A"
            ElseIf myPerfil.TarifaB Then
                TarifaPrecios = "B"
            ElseIf myPerfil.TarifaC Then
                TarifaPrecios = "C"
            ElseIf myPerfil.TarifaD Then
                TarifaPrecios = "D"
            ElseIf myPerfil.TarifaE Then
                TarifaPrecios = "E"
            ElseIf myPerfil.TarifaF Then
                TarifaPrecios = "F"
            Else
                TarifaPrecios = ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM11")
            End If

            AlmacenSalida = myPerfil.Almacen
            If AlmacenSalida = "" Then AlmacenSalida = ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM20")

        End If

    End Sub

    Private Sub dg_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dg.CellContentClick

    End Sub

    Private Sub btnSubirApartado_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSubirApartado.Click
        Dim f As New jsPOSAbono
        f.MontoAbonado = ValorNumero(txtAbonado.Text)
        f.Cargar(myConn)

        If f.Procede Then

        End If

    End Sub

    Private Sub btnBajarApartado_Click(sender As System.Object, e As System.EventArgs) Handles btnBajarApartado.Click

    End Sub

    Private Sub btnRIF_Click(sender As System.Object, e As System.EventArgs) Handles btnRIF.Click
        If EsRIF(txtRIFCI.Text) Then
            If validarRif(txtRIFCI.Text) Then
                Dim f As New jsGenConsultaRIF
                f.DireccionURL = "http://contribuyente.seniat.gob.ve/BuscaRif/BuscaRif.jsp"
                f.RIF = txtRIFCI.Text.Replace("-", "")
                f.NombreEmpresa = ""
                f.Cargar(myConn, "CONSULTA RIF", TipoConsultaWEB.iRIF)
                If f.NombreEmpresa.Trim <> "" Then txtNombre.Text = f.NombreEmpresa
                f.Dispose()
                f = Nothing
            Else
                ft.mensajeCritico("RIF IVALIDO!!!!. VERIFIQUE POR FAVOR")
            End If
        Else
        End If
    End Sub

    Private Sub btnControlEfectivo_Click(sender As System.Object, e As System.EventArgs) Handles btnControlEfectivo.Click
        If CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM42")) Then
            Dim f As New dtSupervisor
            f.Cargar(myConn, ds)
            If f.DialogResult = Windows.Forms.DialogResult.OK Then
                Dim g As New jsPOSRetirosEfectivo
                g.Cargar(myConn, ds, jytsistema.WorkBox)
                g.Dispose()
                g = Nothing
            End If
            f.Close()
            f = Nothing
        End If
    End Sub

    Private Sub btnRetencionIVA_Click(sender As System.Object, e As System.EventArgs) Handles btnRetencionIVA.Click
        If CBool(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM42")) Then
            Dim f As New dtSupervisor
            f.Cargar(myConn, ds)
            If f.DialogResult = Windows.Forms.DialogResult.OK Then
                Dim g As New jsPOSRetencionIVA
                g.Cargar(myConn, ds, jytsistema.WorkBox)
                g.Dispose()
                g = Nothing
            End If
            f.Close()
            f = Nothing
        End If
    End Sub


End Class

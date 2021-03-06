Imports MySql.Data.MySqlClient
Imports Microsoft.Win32
Imports Syncfusion.WinForms.Input
Public Class jsVenArcNotasDebito
    Private Const sModulo As String = "Notas D�bito"
    Private Const nTabla As String = "tblEncabND"
    Private Const lRegion As String = "RibbonButton87"
    Private Const nTablaRenglones As String = "tblRenglones_ND"
    Private Const nTablaIVA As String = "tblIVA"
    Private Const nTablaDescuentos As String = "tblDescuentos"



    Private strSQLMov As String = ""
    Private strSQLIVA As String = ""
    Private strSQLDescuentos As String = ""

    Private myConn As New MySqlConnection(jytsistema.strConn)
    Private ds As New DataSet()
    Private dt As New DataTable
    Private dtRenglones As New DataTable
    Private dtIVA As New DataTable
    Private dtDescuentos As New DataTable
    Private ft As New Transportables

    Private i_modo As Integer
    Private nPosicionEncab As Long, nPosicionRenglon As Long
    Private aTarifa() As String = {"A", "B", "C", "D", "E", "F"}
    Private aImpresa() As String = {"", "I"}

    Private TarifaCliente As String = "A"

    Private Eliminados As New ArrayList

    Private Impresa As Integer

    Private strSQL As String = ""

    Private Sub jsVenArcNotasDebito_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ft = Nothing
    End Sub
    Private Sub jsVenArcNotasDebito_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Me.Dock = DockStyle.Fill
        Try
            myConn.Open()

            strSQL = " (select a.*, b.nombre from jsvenencndb a " _
            & " left join jsvencatcli b on (a.codcli = b.codcli and a.id_emp = b.id_emp) " _
            & " where " _
            & " a.emision >= '" & ft.FormatoFechaMySQL(DateAdd("m", -1 * CInt(ParametroPlus(myConn, Gestion.iVentas, "VENNDBPA24")), jytsistema.sFechadeTrabajo)) & "' and " _
            & " a.emision <= '" & ft.FormatoFechaMySQL(jytsistema.sFechadeTrabajo) & "' and " _
            & " a.id_emp = '" & jytsistema.WorkID & "' order by a.numndb desc) order by numndb "

            ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
            dt = ds.Tables(nTabla)

            DesactivarMarco0()
            If dt.Rows.Count > 0 Then
                nPosicionEncab = dt.Rows.Count - 1
                Me.BindingContext(ds, nTabla).Position = nPosicionEncab
                AsignaTXT(nPosicionEncab)
            Else
                IniciarDocumento(False)
            End If
            ft.ActivarMenuBarra(myConn, ds, dt, lRegion, MenuBarra, jytsistema.sUsuario)
            AsignarTooltips()

            Dim dates As SfDateTimeEdit() = {txtEmision}
            SetSizeDateObjects(dates)

        Catch ex As MySql.Data.MySqlClient.MySqlException
            ft.MensajeCritico("Error en conexi�n de base de datos: " & ex.Message)
        End Try

    End Sub
    Private Sub AsignarTooltips()
        'Menu Barra 
        C1SuperTooltip1.SetToolTip(btnAgregar, "<B>Agregar</B> nueva Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnEditar, "<B>Editar o mofificar</B> Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnEliminar, "<B>Eliminar</B> Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnBuscar, "<B>Buscar</B> Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnPrimero, "ir a la <B>primer</B> Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnSiguiente, "ir a Nota D�bito <B>siguiente</B>")
        C1SuperTooltip1.SetToolTip(btnAnterior, "ir a Nota D�bito <B>anterior</B>")
        C1SuperTooltip1.SetToolTip(btnUltimo, "ir a la <B>�ltimo Nota D�bito</B>")
        C1SuperTooltip1.SetToolTip(btnImprimir, "<B>Imprimir</B> Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnSalir, "<B>Cerrar</B> esta ventana")
        C1SuperTooltip1.SetToolTip(btnDuplicar, "<B>Duplicar</B> este Nota D�bito")

        'Menu barra rengl�n
        C1SuperTooltip1.SetToolTip(btnAgregarMovimiento, "<B>Agregar</B> rengl�n en Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnEditarMovimiento, "<B>Editar</B> rengl�n en Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnEliminarMovimiento, "<B>Eliminar</B> rengl�n en Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnBuscarMovimiento, "<B>Buscar</B> un rengl�n en Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnPrimerMovimiento, "ir al <B>primer</B> rengl�n en Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnAnteriorMovimiento, "ir al <B>anterior</B> rengl�n en Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnSiguienteMovimiento, "ir al rengl�n <B>siguiente </B> en Nota D�bito")
        C1SuperTooltip1.SetToolTip(btnUltimoMovimiento, "ir al <B>�ltimo</B> rengl�n de la Nota D�bito")

    End Sub

    Private Sub AsignaMov(ByVal nRow As Long, ByVal Actualiza As Boolean)
        Dim c As Integer = CInt(nRow)

        If Actualiza Then ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)

        If c >= 0 Then
            Me.BindingContext(ds, nTablaRenglones).Position = c
            MostrarItemsEnMenuBarra(MenuBarraRenglon, c, dtRenglones.Rows.Count)
            dg.Refresh()
            dg.CurrentCell = dg(0, c)
        End If

        ft.ActivarMenuBarra(myConn, ds, dtRenglones, lRegion, MenuBarraRenglon, jytsistema.sUsuario)
        ft.habilitarObjetos(IIf(dtRenglones.Rows.Count > 0, False, True), True, txtCliente, btnCliente)

    End Sub
    Private Sub AsignaTXT(ByVal nRow As Long)

        With dt

            nPosicionEncab = nRow
            Me.BindingContext(ds, nTabla).Position = nRow

            MostrarItemsEnMenuBarra(MenuBarra, nRow, .Rows.Count)

            With .Rows(nRow)
                'Encabezado 
                txtCodigo.Text = ft.MuestraCampoTexto(.Item("numndb"))
                txtEmision.Value = .Item("emision")

                txtFactura.Text = ft.MuestraCampoTexto(.Item("numfac"))
                Dim numControl As String = ft.DevuelveScalarCadena(myConn, " select num_control from jsconnumcon where numdoc = '" & .Item("numndb") & "' and  origen = 'FAC' and org = 'NDB' and id_emp = '" & jytsistema.WorkID & "' ")
                txtControl.Text = IIf(numControl <> "0", numControl, "")

                txtEstatus.Text = IIf(.Item("estatus") = 0, "", "ANULADA")
                txtImpresion.Text = aImpresa(.Item("impresa"))
                txtCliente.Text = ft.MuestraCampoTexto(.Item("codcli"))
                txtComentario.Text = ft.MuestraCampoTexto(.Item("comen"))
                txtAsesor.Text = ft.MuestraCampoTexto(.Item("codven"))
                txtTransporte.Text = ft.MuestraCampoTexto(.Item("transporte"))
                txtAlmacen.Text = ft.MuestraCampoTexto(.Item("almacen"))
                ft.RellenaCombo(aTarifa, cmbTarifa, ft.InArray(aTarifa, .Item("tarifa")))
                txtReferencia.Text = ft.MuestraCampoTexto(.Item("refer"))
                txtCodigoContable.Text = ft.MuestraCampoTexto(.Item("codcon"))

                tslblPesoT.Text = ft.FormatoCantidad(.Item("kilos"))

                txtSubTotal.Text = ft.FormatoNumero(.Item("tot_net"))
                txtTotalIVA.Text = ft.FormatoNumero(.Item("imp_iva"))
                txtTotal.Text = ft.FormatoNumero(.Item("tot_ndb"))

                Impresa = .Item("impresa")

                'Renglones
                AsignarMovimientos(.Item("numndb"))

                'Totales
                CalculaTotales()

            End With
        End With
    End Sub
    Private Sub AsignarMovimientos(ByVal NumeroNotaDebito As String)

        strSQLMov = "select a.*, if(b.descrip is null, '', b.descrip) nomCausa from jsvenrenndb a " _
            & " left join jsvencaudcr b on (a.causa = b.codigo and a.id_emp = b.id_emp and  b.credito_debito = 1) " _
            & " where " _
            & "  " _
            & " a.numndb  = '" & NumeroNotaDebito & "' and " _
            & " a.ejercicio = '" & jytsistema.WorkExercise & "' and " _
            & " a.id_emp = '" & jytsistema.WorkID & "' order by a.renglon "

        ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
        dtRenglones = ds.Tables(nTablaRenglones)

        Dim aCampos() As String = {"item.Item.90.I.", _
                                  "descrip.Descripci�n.300.I.", _
                                  "iva.IVA.30.C.", _
                                  "cantidad.Cantidad.90.D.Cantidad", _
                                  "unidad.UND.35.C.", _
                                  "precio.Precio Unitario.110.D.Numero", _
                                  "des_art.Desc Art�culo.65.D.Numero", _
                                  "des_cli.Desc Cliente.65.D.Numero", _
                                  "des_ofe.Desc Oferta.65.D.Numero", _
                                  "totren.Precio Total.110.D.Numero", _
                                  "causa..50.C.", _
                                  "nomcausa.Causa D�bito.150.I."}

        ft.IniciarTablaPlus(dg, dtRenglones, aCampos)
        If dtRenglones.Rows.Count > 0 Then
            nPosicionRenglon = 0
            AsignaMov(nPosicionRenglon, True)
        End If

    End Sub
    Private Sub AbrirIVA(ByVal NumeroDocumento As String)

        strSQLIVA = "select * from jsvenivandb " _
                            & " where " _
                            & " numndb  = '" & NumeroDocumento & "' and " _
                            & " ejercicio = '" & jytsistema.WorkExercise & "' and " _
                            & " id_emp = '" & jytsistema.WorkID & "' order by tipoiva "

        ds = DataSetRequery(ds, strSQLIVA, myConn, nTablaIVA, lblInfo)
        dtIVA = ds.Tables(nTablaIVA)

        Dim aCampos() As String = {"tipoiva", "poriva", "baseiva", "impiva"}
        Dim aNombres() As String = {"", "", "", ""}
        Dim aAnchos() As Integer = {15, 45, 65, 60}
        Dim aAlineacion() As Integer = {AlineacionDataGrid.Centro, AlineacionDataGrid.Derecha, _
                                        AlineacionDataGrid.Derecha, AlineacionDataGrid.Derecha}

        Dim aFormatos() As String = {"", sFormatoNumero, sFormatoNumero, sFormatoNumero, sFormatoNumero}
        IniciarTabla(dgIVA, dtIVA, aCampos, aNombres, aAnchos, aAlineacion, aFormatos, False, , 8, False)

        txtTotalIVA.Text = ft.FormatoNumero(ft.DevuelveScalarDoble(myConn, " select SUM(IMPIVA) from jsvenivandb where numndb = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' group by numndb "))

    End Sub


    Private Sub IniciarDocumento(ByVal Inicio As Boolean)

        If Inicio Then
            txtCodigo.Text = "NDTMP" & ft.NumeroAleatorio(100000)
        Else
            txtCodigo.Text = ""
        End If

        ft.iniciarTextoObjetos(Transportables.tipoDato.Cadena, txtControl, txtCliente, txtNombreCliente, txtAsesor, txtNombreAsesor, txtComentario, txtTransporte, _
                            txtAlmacen, txtReferencia, txtCodigoContable, txtEstatus, txtFactura, txtImpresion)

        Dim nTransporte As String = ft.DevuelveScalarCadena(myConn, "SELECT codtra FROM jsconctatra WHERE id_emp = '" & jytsistema.WorkID & "' ORDER BY codtra LIMIT 1")
        If nTransporte <> "0" Then txtTransporte.Text = nTransporte

        Dim nAlmacen As String = ft.DevuelveScalarCadena(myConn, "SELECT codalm FROM jsmercatalm WHERE id_emp = '" & jytsistema.WorkID & "' ORDER BY codalm LIMIT 1")
        If nAlmacen <> "0" Then txtAlmacen.Text = nAlmacen

        ft.RellenaCombo(aTarifa, cmbTarifa)
        txtEmision.Value = sFechadeTrabajo

        tslblPesoT.Text = ft.FormatoCantidad(0)

        ft.iniciarTextoObjetos(Transportables.tipoDato.Numero, txtSubTotal, txtTotalIVA, txtTotalIVA)

        Impresa = 0

        'Movimientos
        MostrarItemsEnMenuBarra(MenuBarraRenglon, 0, 0)
        AsignarMovimientos(txtCodigo.Text)
        AbrirIVA(txtCodigo.Text)

    End Sub
    Private Sub ActivarMarco0()

        grpAceptarSalir.Visible = True

        ft.habilitarObjetos(True, False, grpEncab, grpTotales, MenuBarraRenglon)
        ft.habilitarObjetos(True, True, txtComentario, txtEmision, btnCliente, txtCliente, cmbTarifa,
                         btnAsesor, txtAsesor, txtTransporte, btnTransporte, txtAlmacen, btnAlmacen,
                         txtReferencia, txtFactura, btnCodigoContable)

        MenuBarra.Enabled = False
        ft.mensajeEtiqueta(lblInfo, "Haga click sobre cualquier bot�n de la barra menu...", Transportables.TipoMensaje.iAyuda)

    End Sub
    Private Sub DesactivarMarco0()

        grpAceptarSalir.Visible = False
        ft.habilitarObjetos(False, True, txtCodigo, txtEmision, txtEmision, txtControl, txtEstatus,
                txtCliente, txtNombreCliente, btnCliente, txtComentario, txtAsesor, btnAsesor, txtNombreAsesor,
                cmbTarifa, txtTransporte, txtAlmacen, btnTransporte, btnAlmacen, txtFactura,
                txtNombreTransporte, txtNombreAlmacen, txtReferencia, txtCodigoContable, btnCodigoContable, txtImpresion)

        ft.habilitarObjetos(False, True, txtSubTotal, txtTotalIVA, txtTotal)

        MenuBarraRenglon.Enabled = False
        MenuBarra.Enabled = True

        ft.mensajeEtiqueta(lblInfo, "Haga click sobre cualquier bot�n de la barra menu...", Transportables.TipoMensaje.iAyuda)
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click

        If i_modo = movimiento.iAgregar Then
            AgregaYCancela()
        Else
            If dtRenglones.Rows.Count = 0 Then
                ft.mensajeCritico("DEBE INCLUIR AL MENOS UN ITEM...")
                Return
            End If
        End If


        If dt.Rows.Count = 0 Then
            IniciarDocumento(False)
        Else
            Me.BindingContext(ds, nTabla).CancelCurrentEdit()
            AsignaTXT(nPosicionEncab)
        End If

        DesactivarMarco0()
    End Sub
    Private Sub AgregaYCancela()

        ft.Ejecutar_strSQL(myconn, " delete from jsvenrenndb where numndb = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")
        ft.Ejecutar_strSQL(myconn, " delete from jsvenivandb where numndb = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")
        ft.Ejecutar_strSQL(myconn, " delete from jsvenrencom where numdoc = '" & txtCodigo.Text & "' and origen = 'NDV' and id_emp = '" & jytsistema.WorkID & "' ")

    End Sub
    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        If Validado() Then
            GuardarTXT()
            Imprimir()

        End If
    End Sub


    Private Sub Imprimir()

        If DocumentoImpreso(myConn, lblInfo, "jsvenencndb", "numndb", txtCodigo.Text) Then
            Dim f As New jsVenRepParametros
            f.Cargar(TipoCargaFormulario.iShowDialog, ReporteVentas.cNotaDebito, "Nota D�bito", txtCliente.Text, txtCodigo.Text, txtEmision.Value)
            f = Nothing
        Else
            If CBool(ParametroPlus(MyConn, Gestion.iVentas, "VENNDBPA10")) Then
                '1. Imprimir Nota de Entrega Fiscal
                jytsistema.WorkBox = Registry.GetValue(jytsistema.DirReg, jytsistema.ClaveImpresorFiscal, "")
                If jytsistema.WorkBox = "" Then
                    ft.MensajeCritico("DEBE INDICAR UNA FORMA DE IMPRESION FISCAL")
                Else
                    '2. 
                    Dim nTipoImpreFiscal As Integer = TipoImpresoraFiscal(myConn, jytsistema.WorkBox)
                    Select Case nTipoImpreFiscal
                        Case 0 ' FACTURA FISCAL FORMA LIBRE
                            ' SE DIRIGE A LA IMPRESORA POR DEFECTO
                            ImprimirNotaDebitoGrafica(myConn, lblInfo, ds, txtCodigo.Text)
                        Case 1 'FACTURA FISCAL PRE-IMPRESA
                        Case 2, 5, 6  'IMPRESORA FISCAL TIPO ACLAS/BIXOLON
                            ImprimirNotaDebitoFiscalPP1F3(myConn, lblInfo, txtCodigo.Text, NumeroSERIALImpresoraFISCAL(myConn, lblInfo, jytsistema.WorkBox),
                                txtNombreCliente.Text, txtCliente.Text, ft.DevuelveScalarCadena(myConn, "Select RIF from jsvencatcli where codcli = '" & txtCliente.Text & "' and id_emp = '" & jytsistema.WorkID & "'"),
                                ft.DevuelveScalarCadena(myConn, "Select DIRFISCAL from jsvencatcli where codcli = '" & txtCliente.Text & "' and id_emp = '" & jytsistema.WorkID & "'"),
                                txtEmision.Value, "0",
                                txtEmision.Value, txtAsesor.Text, txtNombreAsesor.Text, nTipoImpreFiscal)
                        Case 3 'IMPRESORA FISCAL TIPO BEMATECH
                        Case 4 'IMPRESORA FISCAL TIPO EPSON/PNP
                            ImprimirNotaDebitoFiscalPnP(myConn, lblInfo, txtCodigo.Text, txtNombreCliente.Text, txtCliente.Text,
                                ft.DevuelveScalarCadena(myConn, "Select RIF from jsvencatcli where codcli = '" & txtCliente.Text & "' and id_emp = '" & jytsistema.WorkID & "'"),
                                ft.DevuelveScalarCadena(myConn, "Select DIRFISCAL from jsvencatcli where codcli = '" & txtCliente.Text & "' and id_emp = '" & jytsistema.WorkID & "'"),
                                txtEmision.Value, "0",
                                txtEmision.Value, txtAsesor.Text, txtNombreAsesor.Text)
                    End Select
                End If

                MsgBox("SE HA ENVIADO NOTA DEBITO A LA IMPRESORA FISCAL...", MsgBoxStyle.Information)


            Else
                ft.MensajeCritico("Impresi�n de NOTA DEBITO no permitida...")
            End If
        End If

    End Sub

    Private Function Validado() As Boolean


        If FechaUltimoBloqueo(myConn, "jsvenencndb") >= txtEmision.Value Then
            ft.mensajeCritico("FECHA MENOR QUE ULTIMA FECHA DE CIERRE...")
            Return False
        End If

        If txtNombreCliente.Text = "" Then
            ft.MensajeCritico("Debe indicar un cliente v�lido...")
            Return False
        End If

        If txtNombreAsesor.Text = "" Then
            ft.MensajeCritico("Debe indicar un nombre de Asesor v�lido...")
            Return False
        End If

        If txtNombreTransporte.Text = "" Then
            ft.MensajeCritico("Debe indicar un transporte v�lido...")
            Return False
        End If

        If txtNombreAlmacen.Text = "" Then
            ft.MensajeCritico("Debe indicar un almac�n v�lido...")
            Return False
        End If


        If dtRenglones.Rows.Count = 0 Then
            ft.MensajeCritico("Debe incluir al menos un �tem...")
            Return False
        End If

        Dim EstatusCliente As Integer = ft.DevuelveScalarEntero(myConn, " select estatus from jsvencatcli where codcli = '" & txtCliente.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")
        If EstatusCliente = 1 Or EstatusCliente = 2 Then
            ft.MensajeCritico("Este Cliente posee estatus  " & aEstatusCliente(EstatusCliente) & ". Favor remitir a Administraci�n")
            Return False
        End If

        If dtRenglones.Rows.Count <= 0 Then
            ft.MensajeCritico("Debe introducir por lo menos un �tem")
            Return False
        End If

        Return True

    End Function
    Private Sub GuardarTXT()

        Dim Codigo As String = txtCodigo.Text
        Dim Inserta As Boolean = False
        Dim aEstatus() As String = {"", "ANULADA"}
        If i_modo = movimiento.iAgregar Then
            Inserta = True
            nPosicionEncab = ds.Tables(nTabla).Rows.Count
            Codigo = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMNDB", "08")

            ft.Ejecutar_strSQL(myconn, " update jsvenrenndb set numndb = '" & Codigo & "' where numndb = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")
            ft.Ejecutar_strSQL(myconn, " update jsvenivandb set numndb = '" & Codigo & "' where numndb = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")
            ft.Ejecutar_strSQL(myconn, " update jsvenrencom set numdoc = '" & Codigo & "' where numdoc = '" & txtCodigo.Text & "' and origen = 'NDV' and id_emp = '" & jytsistema.WorkID & "' ")

        End If

        InsertEditVENTASEncabezadoNOTADEBITO(myConn, lblInfo, Inserta, Codigo, txtFactura.Text, txtEmision.Value, txtCliente.Text,
                                               txtComentario.Text, txtAsesor.Text, txtAlmacen.Text, txtTransporte.Text,
                                               txtReferencia.Text, txtCodigoContable.Text, cmbTarifa.Text,
                                               ValorEntero(dtRenglones.Rows.Count), 0.0, ValorCantidad(tslblPesoT.Text),
                                               ValorNumero(txtSubTotal.Text), ValorNumero(txtTotalIVA.Text), ValorNumero(txtTotal.Text),
                                               txtEmision.Value, IIf(i_modo = movimiento.iAgregar, 1, ft.InArray(aEstatus, txtEstatus.Text)),
                                               "", jytsistema.sFechadeTrabajo, Impresa, "")


        ActualizarMovimientos(Codigo)

        ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
        dt = ds.Tables(nTabla)


        Dim row As DataRow = dt.Select(" numndb = '" & Codigo & "' AND ID_EMP = '" & jytsistema.WorkID & "' ")(0)
        nPosicionEncab = dt.Rows.IndexOf(row)

        Me.BindingContext(ds, nTabla).Position = nPosicionEncab
        AsignaTXT(nPosicionEncab)
        DesactivarMarco0()
        ft.ActivarMenuBarra(myConn, ds, dt, lRegion, MenuBarra, jytsistema.sUsuario)

    End Sub
    Private Sub ActualizarMovimientos(ByVal NumeroNotaDebito As String)


        '1.- Elimina movimientos de inventarios anteriores de la Factura
        EliminarMovimientosdeInventario(myConn, NumeroNotaDebito, "NDV", lblInfo)

        '2.- Actualizar Movimientos de Inventario con Nota D�bito
        strSQLMov = "select a.*, if(b.descrip is null, '', b.descrip) nomCausa from jsvenrenndb a " _
            & " left join jsvencaudcr b on (a.causa = b.codigo and a.id_emp = b.id_emp and  b.credito_debito = 1) " _
            & " where " _
            & "  " _
            & " a.numndb  = '" & NumeroNotaDebito & "' and " _
            & " a.ejercicio = '" & jytsistema.WorkExercise & "' and " _
            & " a.id_emp = '" & jytsistema.WorkID & "' order by a.renglon "

        ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
        dtRenglones = ds.Tables(nTablaRenglones)

        For Each dtRow As DataRow In dtRenglones.Rows
            With dtRow
                Dim CostoActual As Double = UltimoCostoAFecha(myConn, .Item("item"), txtEmision.Value) / Equivalencia(myConn, .Item("item"), .Item("unidad"))

                Dim CausaDebito As String = ft.DevuelveScalarCadena(myConn, " select codigo from jsvencaudcr where codigo = '" & .Item("causa") & "' and credito_debito = 1 and id_emp = '" & jytsistema.WorkID & "' ")
                Dim DevBuenEstado As Boolean = ft.DevuelveScalarBooleano(myConn, " select estado from jsvencaudcr where codigo = '" & .Item("causa") & "' and credito_debito = 1 and id_emp = '" & jytsistema.WorkID & "' ")

                If CausaDebito <> "0" Then

                    Dim MueveInventario As Boolean = ft.DevuelveScalarBooleano(myConn, " select inventario from jsvencaudcr where codigo = '" & .Item("causa") & "' and credito_debito = 1 and id_emp = '" & jytsistema.WorkID & "' ")
                    If MueveInventario Then _
                        InsertEditMERCASMovimientoInventario(myConn, lblInfo, True, .Item("item"), txtEmision.Value, "SA", NumeroNotaDebito,
                                                             .Item("unidad"), .Item("cantidad"), .Item("peso"), .Item("cantidad") * CostoActual,
                                                             .Item("cantidad") * CostoActual, "NDV", NumeroNotaDebito, .Item("lote"), txtCliente.Text,
                                                             .Item("totren"), .Item("totrendes"), 0, .Item("totren") - .Item("totrendes"), txtAsesor.Text,
                                                              If(DevBuenEstado, .Item("almacen"), "00002"), .Item("renglon"), jytsistema.sFechadeTrabajo)

                    Dim AjustaPrecio As Boolean = ft.DevuelveScalarBooleano(myConn, " select ajustaprecio from jsvencaudcr where codigo = '" & .Item("causa") & "' and credito_debito = 1 and id_emp = '" & jytsistema.WorkID & "' ")
                    If AjustaPrecio Then _
                        InsertEditMERCASMovimientoInventario(myConn, lblInfo, True, .Item("item"), txtEmision.Value, "AP", NumeroNotaDebito,
                                                                             .Item("unidad"), 0.0, 0.0, 0.0,
                                                                             0.0, "NDV", .Item("numfac"), .Item("lote"), txtCliente.Text,
                                                                             .Item("totren"), .Item("totrendes"), 0, 0.0, txtAsesor.Text,
                                                                              If(DevBuenEstado, .Item("almacen"), "00002"), .Item("renglon"), jytsistema.sFechadeTrabajo)
                Else
                    InsertEditMERCASMovimientoInventario(myConn, lblInfo, True, .Item("item"), txtEmision.Value, "SA", NumeroNotaDebito,
                                                         .Item("unidad"), .Item("cantidad"), .Item("peso"), .Item("cantidad") * CostoActual,
                                                         .Item("cantidad") * CostoActual, "NDV", NumeroNotaDebito, IIf(IsDBNull(.Item("lote")), "", .Item("lote")), txtCliente.Text,
                                                         .Item("totren"), .Item("totrendes"), 0, .Item("totren") - .Item("totrendes"), txtAsesor.Text,
                                                          If(DevBuenEstado, .Item("almacen"), "00002"), .Item("renglon"), jytsistema.sFechadeTrabajo)
                End If

                ActualizarExistenciasPlus(myConn, .Item("item"))
            End With
        Next

        '4.- Actualizar existencias de renglones eliminados
        For Each aSTR As Object In Eliminados
            ActualizarExistenciasPlus(myConn, aSTR)
        Next

        '5.- Actualizar CxC 
        ft.Ejecutar_strSQL(myconn, " DELETE from jsventracob WHERE " _
                                            & " TIPOMOV = 'ND' AND  " _
                                            & " NUMMOV = '" & NumeroNotaDebito & "' AND " _
                                            & " ORIGEN = 'NDB' AND " _
                                            & " EJERCICIO = '" & jytsistema.WorkExercise & "' AND " _
                                            & " ID_EMP = '" & jytsistema.WorkID & "'  ")

        InsertEditVENTASCXC(myConn, lblInfo, True, txtCliente.Text, "ND", NumeroNotaDebito, txtEmision.Value, ft.FormatoHora(Now),
                            txtEmision.Value, txtReferencia.Text, "Nota D�bito : " & NumeroNotaDebito, ValorNumero(txtTotal.Text),
                            ValorNumero(txtTotalIVA.Text), "", "", "", "", "", "NDB", NumeroNotaDebito, "0", "", jytsistema.sFechadeTrabajo,
                            txtCodigoContable.Text, "", "", 0.0, 0.0, "", "", "", "", txtAsesor.Text, txtAsesor.Text, "0", "0", "")


    End Sub

    Private Sub txtNombre_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtComentario.GotFocus
        ft.mensajeEtiqueta(lblInfo, " Indique comentario ... ", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub btnAgregar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregar.Click
        i_modo = movimiento.iAgregar
        nPosicionEncab = Me.BindingContext(ds, nTabla).Position
        ActivarMarco0()
        IniciarDocumento(True)
        ft.habilitarObjetos(IIf(dtRenglones.Rows.Count > 0, False, True), True, txtCliente, btnCliente)
    End Sub

    Private Sub btnEditar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditar.Click

        Dim aCamposAdicionales() As String = {"numndb|'" & txtCodigo.Text & "'", _
                                             "codcli|'" & txtCliente.Text & "'"}
        If DocumentoBloqueado(myConn, "jsvenencndb", aCamposAdicionales) Then
            ft.mensajeCritico("DOCUMENTO BLOQUEADO. POR FAVOR VERIFIQUE...")
        Else
            If CBool(ParametroPlus(myConn, Gestion.iVentas, "VENNDBPA01")) Then
                If dt.Rows(nPosicionEncab).Item("RELFACTURAS") = "" Then
                    i_modo = movimiento.iEditar
                    nPosicionEncab = Me.BindingContext(ds, nTabla).Position
                    ActivarMarco0()
                    ft.habilitarObjetos(IIf(dtRenglones.Rows.Count > 0, False, True), True, txtCliente, btnCliente)
                Else
                    ft.mensajeCritico("Esta NOTA DEBITO pertenece a relaci�n de facturas N� " + dt.Rows(nPosicionEncab).Item("RELFACTURAS") + vbLf _
                                      + "por lo tanto su MODIFICACION no est� permitida")
                End If
            Else
                ft.mensajeCritico("Edici�n de Notas D�bito NO est� permitida...")
            End If
        End If
    End Sub

    Private Sub btnEliminar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEliminar.Click
        Dim aCamposAdicionales() As String = {"numndb|'" & txtCodigo.Text & "'", _
                                             "codcli|'" & txtCliente.Text & "'"}
        If DocumentoBloqueado(myConn, "jsvenencndb", aCamposAdicionales) Then
            ft.mensajeCritico("DOCUMENTO BLOQUEADO. POR FAVOR VERIFIQUE...")
        Else
            If CBool(ParametroPlus(myConn, Gestion.iVentas, "VENNDBPA01")) Then
                Dim sRespuesta As Microsoft.VisualBasic.MsgBoxResult
                nPosicionEncab = Me.BindingContext(ds, nTabla).Position

                If dt.Rows(nPosicionEncab).Item("RELFACTURAS") = "" Then
                    sRespuesta = MsgBox(" � Est� seguro que desea eliminar registro ?", MsgBoxStyle.YesNo, sModulo & " Eliminar registro ... ")

                    If sRespuesta = MsgBoxResult.Yes Then

                        InsertarAuditoria(myConn, MovAud.iEliminar, sModulo, dt.Rows(nPosicionEncab).Item("numndb"))
                        For Each dtRow As DataRow In dtRenglones.Rows
                            With dtRow
                                Eliminados.Add(.Item("item"))
                            End With
                        Next

                        ft.Ejecutar_strSQL(myConn, " delete from jsvenencndb where numndb = '" & txtCodigo.Text & "' AND ID_EMP = '" & jytsistema.WorkID & "'")

                        ft.Ejecutar_strSQL(myConn, " delete from jsvenrenndb where numndb = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")
                        ft.Ejecutar_strSQL(myConn, " delete from jsvenivandb where numndb = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")
                        ft.Ejecutar_strSQL(myConn, " delete from jsvenrencom where numdoc = '" & txtCodigo.Text & "' and origen = 'NDV' and id_emp = '" & jytsistema.WorkID & "' ")
                        ft.Ejecutar_strSQL(myConn, " delete from jsventracob where nummov = '" & txtCodigo.Text & "' and origen = 'NDB' AND Ejercicio = '" & jytsistema.WorkExercise & "' AND ID_EMP = '" & jytsistema.WorkID & "'")

                        'ELIMINIAR DE CHEQUES DEVUELTOS
                        ft.Ejecutar_strSQL(myConn, " delete from jsbanchedev where numcheque = '" & txtReferencia.Text & "' and prov_cli = '" & txtCliente.Text & "' and fechadev = '" & ft.FormatoFechaMySQL(txtEmision.Value) & "' AND ID_EMP = '" & jytsistema.WorkID & "'")
                        'ELIMINAR MOVIMIENTO EN BANCOS DE CHEQUE DEVUELTO
                        ft.Ejecutar_strSQL(myConn, " delete from jsbantraban where numdoc = '" & txtReferencia.Text & "' and numorg = '" & txtReferencia.Text & "' and prov_cli = '" & txtCliente.Text & "' and fechamov = '" & ft.FormatoFechaMySQL(txtEmision.Value) & "' AND ID_EMP = '" & jytsistema.WorkID & "'")


                        EliminarMovimientosdeInventario(myConn, txtCodigo.Text, "NDV", lblInfo)

                        For Each aSTR As Object In Eliminados
                            ActualizarExistenciasPlus(myConn, aSTR)
                        Next

                        ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
                        dt = ds.Tables(nTabla)
                        If dt.Rows.Count - 1 < nPosicionEncab Then nPosicionEncab = dt.Rows.Count - 1
                        AsignaTXT(nPosicionEncab)

                    End If
                Else
                    ft.mensajeCritico("Esta NOTA DEBITO pertenece a relaci�n de facturas N� " + dt.Rows(nPosicionEncab).Item("RELFACTURAS") + vbLf _
                                      + "por lo tanto su eliminaci�n no est� permitida")
                End If
            Else
                ft.mensajeCritico("Eliminaci�n de Notas D�bito NO est� permitida...")
            End If
        End If
    End Sub

    Private Sub btnBuscar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuscar.Click
        Dim f As New frmBuscar
        Dim Campos() As String = {"numndb", "nombre", "emision", "comen"}
        Dim Nombres() As String = {"N�mero ", "Nombre", "Emisi�n", "Comentario"}
        Dim Anchos() As Integer = {150, 400, 100, 150}
        f.Buscar(dt, Campos, Nombres, Anchos, Me.BindingContext(ds, nTabla).Position, "Notas d�bito ventas...")
        AsignaTXT(f.Apuntador)
        f = Nothing
    End Sub

    Private Sub btnPrimero_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrimero.Click
        Me.BindingContext(ds, nTabla).Position = 0
        AsignaTXT(Me.BindingContext(ds, nTabla).Position)
    End Sub

    Private Sub btnAnterior_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAnterior.Click
        Me.BindingContext(ds, nTabla).Position -= 1
        AsignaTXT(Me.BindingContext(ds, nTabla).Position)
    End Sub

    Private Sub btnSiguiente_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSiguiente.Click
        Me.BindingContext(ds, nTabla).Position += 1
        AsignaTXT(Me.BindingContext(ds, nTabla).Position)
    End Sub

    Private Sub btnUltimo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUltimo.Click
        Me.BindingContext(ds, nTabla).Position = ds.Tables(nTabla).Rows.Count - 1
        AsignaTXT(Me.BindingContext(ds, nTabla).Position)
    End Sub

    Private Sub btnSalir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSalir.Click
        Me.Close()
    End Sub

    Private Sub dg_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles dg.RowHeaderMouseClick, _
       dg.CellMouseClick
        Me.BindingContext(ds, nTablaRenglones).Position = e.RowIndex
        MostrarItemsEnMenuBarra(MenuBarraRenglon, e.RowIndex, ds.Tables(nTablaRenglones).Rows.Count)
    End Sub

    Private Sub CalculaTotales()

        txtSubTotal.Text = ft.FormatoNumero(CalculaTotalRenglonesVentas(myConn, lblInfo, "jsvenrenndb", "numndb", "totren", txtCodigo.Text, 0))
        CalculaTotalIVAVentas(myConn, lblInfo, "", "jsvenivandb", "jsvenrenndb", "numndb", txtCodigo.Text, "impiva", "totrendes", txtEmision.Value, "totren")
        AbrirIVA(txtCodigo.Text)
        txtTotal.Text = ft.FormatoNumero(ValorNumero(txtSubTotal.Text) + ValorNumero(txtTotalIVA.Text))

        tslblPesoT.Text = ft.FormatoCantidad(CalculaPesoDocumento(myConn, lblInfo, "jsvenrenndb", "peso", "numndb", txtCodigo.Text))

    End Sub

    Private Sub btnAgregarMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregarMovimiento.Click
        If txtNombreCliente.Text <> "" Then
            Dim f As New jsGenRenglonesMovimientos
            f.Apuntador = Me.BindingContext(ds, nTablaRenglones).Position
            f.Agregar(myConn, ds, dtRenglones, "NDV", txtCodigo.Text, txtEmision.Value, txtAlmacen.Text, cmbTarifa.Text, txtCliente.Text, , , , , , , , , , txtAsesor.Text)
            ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
            AsignaMov(f.Apuntador, True)
            CalculaTotales()
            f = Nothing
        End If
    End Sub

    Private Sub btnEditarMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditarMovimiento.Click

        If dtRenglones.Rows.Count > 0 Then
            Dim f As New jsGenRenglonesMovimientos
            f.Apuntador = Me.BindingContext(ds, nTablaRenglones).Position
            f.Editar(myConn, ds, dtRenglones, "NDV", txtCodigo.Text, txtEmision.Value, txtAlmacen.Text, cmbTarifa.Text, txtCliente.Text,
                     IIf(dtRenglones.Rows(Me.BindingContext(ds, nTablaRenglones).Position).Item("item").ToString.Substring(0, 1) = "$", True, False), , , , , , , , , txtAsesor.Text)
            ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
            AsignaMov(f.Apuntador, True)
            CalculaTotales()
            f = Nothing
        End If

    End Sub

    Private Sub btnEliminarMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEliminarMovimiento.Click

        EliminarMovimiento()

    End Sub
    Private Sub EliminarMovimiento()

        Dim sRespuesta As Microsoft.VisualBasic.MsgBoxResult
        nPosicionRenglon = Me.BindingContext(ds, nTablaRenglones).Position

        If nPosicionRenglon >= 0 Then
            sRespuesta = MsgBox(" � Est� seguro que desea eliminar registro ?", MsgBoxStyle.YesNo, "Eliminar registro ... ")
            If sRespuesta = MsgBoxResult.Yes Then
                With dtRenglones.Rows(nPosicionRenglon)
                    InsertarAuditoria(myConn, MovAud.iEliminar, sModulo, .Item("item"))

                    Eliminados.Add(.Item("item"))

                    Dim aCamposDel() As String = {"numndb", "item", "renglon", "id_emp"}
                    Dim aStringsDel() As String = {txtCodigo.Text, .Item("item"), .Item("renglon"), jytsistema.WorkID}

                    ft.Ejecutar_strSQL(myconn, " delete from jsvenrencom where numdoc = '" & txtCodigo.Text & "' and " _
                           & " origen = 'NDV' and item = '" & .Item("item") & "' and renglon = '" & .Item("renglon") & "' and " _
                           & " id_emp = '" & jytsistema.WorkID & "' ")

                    nPosicionRenglon = EliminarRegistros(myConn, lblInfo, ds, nTablaRenglones, "jsvenrenndb", strSQLMov, aCamposDel, aStringsDel, _
                                                  Me.BindingContext(ds, nTablaRenglones).Position, True)

                    If dtRenglones.Rows.Count - 1 < nPosicionRenglon Then nPosicionRenglon = dtRenglones.Rows.Count - 1
                    AsignaMov(nPosicionRenglon, True)
                    CalculaTotales()
                End With
            End If
        End If

    End Sub
    Private Sub btnBuscarMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuscarMovimiento.Click

        Dim f As New frmBuscar
        Dim Campos() As String = {"item", "descripcion"}
        Dim Nombres() As String = {"Item", "Descripci�n"}
        Dim Anchos() As Integer = {140, 350}
        f.Text = "Movimientos "
        f.Buscar(dtRenglones, Campos, Nombres, Anchos, Me.BindingContext(ds, nTablaRenglones).Position, " " & Me.Tag & "...")
        AsignaMov(f.Apuntador, False)
        f = Nothing

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

    Private Sub btnImprimir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnImprimir.Click
        Imprimir()
    End Sub

    Private Sub btnAsesor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAsesor.Click
        If CBool(ParametroPlus(myConn, Gestion.iVentas, "VENNDBPA11")) Then
            txtAsesor.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codven codigo, CONCAT( apellidos, ', ', nombres) descripcion from jsvencatven where tipo = '" & TipoVendedor.iFuerzaventa & "'  and estatus = 1 and id_emp = '" & jytsistema.WorkID & "'  order by 1 ", "Asesores Comerciales",
                                               txtAsesor.Text)
        Else
            ft.mensajeCritico("Escogencia de asesor comercial no permitida...")
        End If
    End Sub

    Private Sub txtCliente_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtCliente.TextChanged
        If txtCliente.Text <> "" Then
            Dim aFld() As String = {"codcli", "id_emp"}
            Dim aStr() As String = {txtCliente.Text, jytsistema.WorkID}
            Dim FormaDePagoCliente As String = qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "formapago")
            txtNombreCliente.Text = qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "nombre")


            Dim mTotalFac As Double = ValorNumero(txtTotal.Text)
            If i_modo = movimiento.iAgregar Then
                mTotalFac = 0.0

                If qFound(myConn, lblInfo, "jsvencatcli", aFld, aStr) Then
                    cmbTarifa.SelectedIndex = ft.InArray(aTarifa, qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "tarifa"))
                    Dim cAsesor As String = ft.DevuelveScalarCadena(myConn, "SELECT b.codven  " _
                                                            & " FROM jsvenrenrut a  " _
                                                            & " LEFT JOIN jsvenencrut b ON (a.codrut = b.codrut AND a.tipo = b.tipo AND a.id_emp = b.id_emp) " _
                                                            & " WHERE " _
                                                            & " a.tipo = '0' AND " _
                                                            & " a.cliente = '" & txtCliente.Text & "' AND " _
                                                            & " a.id_emp = '" & jytsistema.WorkID & "' ")

                    txtAsesor.Text = cAsesor
                    If qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "transporte") <> "" Then txtTransporte.Text = qFoundAndSign(myConn, lblInfo, "jsvencatcli", aFld, aStr, "transporte")
                End If

            End If

        End If
    End Sub

    Private Sub dgIVA_CellFormatting(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellFormattingEventArgs) Handles _
        dgIVA.CellFormatting
        If e.ColumnIndex = 1 Then e.Value = ft.FormatoNumero(e.Value) & "%"
    End Sub

    Private Sub txtAsesor_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAsesor.TextChanged
        Dim aFld() As String = {"codven", "tipo", "id_emp"}
        Dim aStr() As String = {txtAsesor.Text, "0", jytsistema.WorkID}
        txtNombreAsesor.Text = qFoundAndSign(myConn, lblInfo, "jsvencatven", aFld, aStr, "apellidos") & ", " _
                & qFoundAndSign(myConn, lblInfo, "jsvencatven", aFld, aStr, "nombres")
    End Sub

    Private Sub btnCliente_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCliente.Click
        txtCliente.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codcli codigo, nombre descripcion, disponible, elt(estatus+1, 'Activo', 'Bloqueado', 'Inactivo', 'Desincorporado') estatus from jsvencatcli where estatus < 3 and id_emp = '" & jytsistema.WorkID & "' order by 1 ", "Clientes", _
                                            txtCliente.Text)
    End Sub

    Private Sub btnAgregarServicio_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregarServicio.Click
        If txtNombreCliente.Text <> "" Then
            Dim f As New jsGenRenglonesMovimientos
            f.Apuntador = Me.BindingContext(ds, nTablaRenglones).Position
            f.Agregar(myConn, ds, dtRenglones, "NDV", txtCodigo.Text, txtEmision.Value, txtAlmacen.Text, cmbTarifa.Text, txtCliente.Text, True)
            ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
            AsignaMov(f.Apuntador, True)
            CalculaTotales()
            f = Nothing
        End If
    End Sub


    Private Sub txtTransporte_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTransporte.TextChanged
        txtNombreTransporte.Text = ft.DevuelveScalarCadena(myConn, " select nomtra from jsconctatra where codtra = '" & txtTransporte.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")
    End Sub

    Private Sub txtAlmacen_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAlmacen.TextChanged
        txtNombreAlmacen.Text = ft.DevuelveScalarCadena(myConn, " select desalm from jsmercatalm where codalm = '" & txtAlmacen.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")
    End Sub

    Private Sub btnTransporte_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTransporte.Click
        txtTransporte.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codtra codigo, nomtra descripcion from jsconctatra where id_emp = '" & jytsistema.WorkID & "' order by 1 ", "Transportes", _
                                               txtTransporte.Text)
    End Sub

    Private Sub btnAlmacen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAlmacen.Click
        txtAlmacen.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codalm codigo, desalm descripcion from jsmercatalm where id_emp = '" & jytsistema.WorkID & "' order by 1 ", "Almacenes", _
                                            txtAlmacen.Text)
    End Sub

    Private Sub btnCodigoContable_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCodigoContable.Click
        txtCodigoContable.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codcon codigo, descripcion from jscotcatcon where marca = 0 and id_emp = '" & jytsistema.WorkID & "' order by 1 ", "Cuentas Contables", _
                                                   txtCodigoContable.Text)
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


    Private Sub btnCortar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCortar.Click
        If dtRenglones.Rows.Count > 0 Then
            nPosicionRenglon = Me.BindingContext(ds, nTablaRenglones).Position
            With dtRenglones.Rows(nPosicionRenglon)
                Dim f As New jsGenCambioPrecioEnAlbaranes
                f.NuevoPrecio = .Item("precio")
                Dim PrecioAnterior As Double = f.NuevoPrecio
                f.Cambiar(myConn, ds, dtRenglones, txtCodigo.Text, txtCliente.Text, .Item("renglon"), .Item("item"), .Item("unidad"), .Item("cantidad"), .Item("precio"))
                If f.NuevoPrecio <> PrecioAnterior Then

                    ft.Ejecutar_strSQL(myconn, " update jsvenrenndb " _
                                   & " set precio = " & f.NuevoPrecio & ", " _
                                   & " totren = " & f.NuevoPrecio * .Item("cantidad") & ", " _
                                   & " totrendes = " & f.NuevoPrecio * .Item("cantidad") * (1 - .Item("des_art") / 100) * (1 - .Item("des_cli") / 100) * (1 - .Item("des_ofe") / 100) & " " _
                                   & " where " _
                                   & " numndb = '" & txtCodigo.Text & "' and " _
                                   & " renglon = '" & .Item("renglon") & "' and " _
                                   & " item = '" & .Item("item") & "' and " _
                                   & " id_emp = '" & jytsistema.WorkID & "' ")

                    ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
                    AsignaMov(nPosicionRenglon, True)
                    CalculaTotales()

                End If

                f = Nothing
            End With
        End If
    End Sub

End Class
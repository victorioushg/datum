Imports MySql.Data.MySqlClient
Imports Syncfusion.WinForms.Input
Public Class jsVenProGuiaPedidos
    Private Const sModulo As String = "Guia De PEDIDOS"
    Private Const nTabla As String = "tblEncabGuiaDespacho"
    Private Const lRegion As String = "RibbonButton273"
    Private Const nTablaRenglones As String = "tblRenglones_GuiaPedidos"

    Private strSQL As String = "select * from jsvenencguipedidos where ID_EMP = '" _
        & jytsistema.WorkID & "' and " _
        & "EJERCICIO = '" & jytsistema.WorkExercise & "' order by codigoguia "

    Private strSQLMov As String = ""
    Private strSQLIVA As String = ""
    Private strSQLDescuentos As String = ""

    Private myConn As New MySqlConnection(jytsistema.strConn)
    Private ds As New DataSet()
    Private dt As New DataTable
    Private dtRenglones As New DataTable
    Private ft As New Transportables

    Private i_modo As Integer
    Private nPosicionEncab As Long, nPosicionRenglon As Long, nPosicionDescuento As Long

    Private aEstatus() As String = {"Activa", "Procesada"}

    Private Sub jsVenProGuiaPedidos_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ft = Nothing
    End Sub

    Private Sub jsVenProGuiaDespacho_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Me.Dock = DockStyle.Fill
        Me.Tag = sModulo
        Try
            myConn.Open()

            ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
            dt = ds.Tables(nTabla)

            Dim dates As SfDateTimeEdit() = {txtEmision, txtFacturaDesde, txtFacturaHasta}
            SetSizeDateObjects(dates)

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

        Catch ex As MySql.Data.MySqlClient.MySqlException
            ft.MensajeCritico("Error en conexi�n de base de datos: " & ex.Message)
        End Try

    End Sub
    Private Sub AsignarTooltips()
        'Menu Barra 
        C1SuperTooltip1.SetToolTip(btnAgregar, "<B>Agregar</B> nueva Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnEditar, "<B>Editar o mofificar</B> Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnEliminar, "<B>Eliminar</B> Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnBuscar, "<B>Buscar</B> Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnPrimero, "ir a la <B>primer</B> Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnSiguiente, "ir a Gu�a de Pedidos <B>siguiente</B>")
        C1SuperTooltip1.SetToolTip(btnAnterior, "ir a Gu�a de Pedidos <B>anterior</B>")
        C1SuperTooltip1.SetToolTip(btnUltimo, "ir a la <B>�ltimo Gu�a de Pedidos</B>")

        C1SuperTooltip1.SetToolTip(btnSalir, "<B>Cerrar</B> esta ventana")

        'Menu barra rengl�n
        C1SuperTooltip1.SetToolTip(btnAgregarMovimiento, "<B>Agregar</B> rengl�n en Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnEditarMovimiento, "<B>Editar</B> rengl�n en Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnEliminarMovimiento, "<B>Eliminar</B> rengl�n en Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnBuscarMovimiento, "<B>Buscar</B> un rengl�n en Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnPrimerMovimiento, "ir al <B>primer</B> rengl�n en Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnAnteriorMovimiento, "ir al <B>anterior</B> rengl�n en Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnSiguienteMovimiento, "ir al rengl�n <B>siguiente </B> en Gu�a de Pedidos")
        C1SuperTooltip1.SetToolTip(btnUltimoMovimiento, "ir al <B>�ltimo</B> rengl�n de la Gu�a de Pedidos")

    End Sub

    Private Sub AsignaMov(ByVal nRow As Long, ByVal Actualiza As Boolean)
        Dim c As Integer = CInt(nRow)
        If Actualiza Then ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
        dtRenglones = ds.Tables(nTablaRenglones)
        If c >= 0 Then
            Me.BindingContext(ds, nTablaRenglones).Position = c
            MostrarItemsEnMenuBarra(MenuBarraRenglon, c, dtRenglones.Rows.Count)
            dg.Refresh()
            dg.CurrentCell = dg(0, c)
        End If

        ft.ActivarMenuBarra(myConn, ds, dtRenglones, lRegion, MenuBarraRenglon, jytsistema.sUsuario)

    End Sub
    Private Sub AsignaTXT(ByVal nRow As Long)

        With dt

            nPosicionEncab = nRow
            Me.BindingContext(ds, nTabla).Position = nRow

            MostrarItemsEnMenuBarra(MenuBarra, nRow, .Rows.Count)

            If nRow >= 0 Then
                With .Rows(nRow)
                    'Encabezado 
                    txtCodigo.Text = .Item("codigoguia")
                    txtEmision.Value = .Item("fechaguia")
                    txtUsuario.Text = .Item("elaborador")
                    txtEstatus.Text = aEstatus(.Item("estatus"))
                    txtComentario.Text = ft.MuestraCampoTexto(.Item("descripcion"))
                    txtTransporte.Text = ft.MuestraCampoTexto(.Item("transporte"))

                    tslblPesoT.Text = ft.FormatoCantidad(.Item("totalkilos"))
                    txtTotal.Text = ft.FormatoNumero(.Item("totalguia"))

                    txtFacturaDesde.Value = .Item("emisionfac")
                    txtFacturaHasta.Value = .Item("hastafac")

                    'Renglones
                    AsignarMovimientos(.Item("codigoguia"))

                    'Totales
                    CalculaTotales()

                End With
            End If

        End With
    End Sub
    Private Sub AsignarMovimientos(ByVal CodigoGuia As String)

        strSQLMov = "select a.*, elt(b.condpag+1,'CR','CO') as condpag  " _
                        & " from jsvenrenguipedidos a " _
                        & " left join jsvenencped b on (a.codigofac = b.numped and a.id_emp = b.id_emp)" _
                        & " where a.CODIGOGUIA = '" & CodigoGuia & "' and " _
                        & " a.ID_EMP = '" & jytsistema.WorkID & "' and " _
                        & " a.EJERCICIO = '" & jytsistema.WorkExercise & "' " _
                        & " order by a.CODVEN, a.CODIGOFAC "

        ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
        dtRenglones = ds.Tables(nTablaRenglones)

        Dim aCampos() As String = {"codigofac", "nomcli", "condpag", "totalfac", "kilosfac", ""}
        Dim aNombres() As String = {"N�mero PEDIDO", "Nombre Cliente", "Condici�n Pago", "Total PEDIDO", "Peso PEDIDO (Kgs)", ""}
        Dim aAnchos() As Integer = {150, 400, 70, 120, 120, 100}
        Dim aAlineacion() As Integer = {AlineacionDataGrid.Izquierda, AlineacionDataGrid.Izquierda,
                                        AlineacionDataGrid.Centro, AlineacionDataGrid.Derecha, AlineacionDataGrid.Derecha,
                                        AlineacionDataGrid.Izquierda}

        Dim aFormatos() As String = {"", "", "", sFormatoNumero, sFormatoCantidad, ""}
        IniciarTabla(dg, dtRenglones, aCampos, aNombres, aAnchos, aAlineacion, aFormatos)
        If dtRenglones.Rows.Count > 0 Then
            nPosicionRenglon = 0
            AsignaMov(nPosicionRenglon, True)
        End If

    End Sub


    Private Sub IniciarDocumento(ByVal Inicio As Boolean)

        If Inicio Then
            txtCodigo.Text = "GUIATMP" & ft.NumeroAleatorio(100000)
        Else
            txtCodigo.Text = ""
        End If


        txtUsuario.Text = ""
        txtNombreUsuario.Text = ""
        txtTransporte.Text = ""
        txtComentario.Text = ""
        txtEstatus.Text = aEstatus(0)
        txtEmision.Value = jytsistema.sFechadeTrabajo
        tslblPesoT.Text = ft.FormatoCantidad(0)

        txtTotal.Text = ft.FormatoNumero(0.0)
        txtFacturaDesde.Value = jytsistema.sFechadeTrabajo
        txtFacturaHasta.Value = jytsistema.sFechadeTrabajo
        txtChoferTransporte.Text = ""


        'Movimientos
        MostrarItemsEnMenuBarra(MenuBarraRenglon, 0, 0)
        AsignarMovimientos(txtCodigo.Text)


    End Sub
    Private Sub ActivarMarco0()

        grpAceptarSalir.Visible = True

        ft.habilitarObjetos(True, False, grpEncab, grpTotales, MenuBarraRenglon)
        ft.habilitarObjetos(True, True, txtComentario, txtEmision, btnTransporte, txtFacturaHasta, txtFacturaDesde,
                         btnTransporte)
        MenuBarra.Enabled = False
        ft.mensajeEtiqueta(lblInfo, "Haga click sobre cualquier bot�n de la barra menu...", Transportables.TipoMensaje.iAyuda)

    End Sub
    Private Sub DesactivarMarco0()

        grpAceptarSalir.Visible = False
        ft.habilitarObjetos(False, True, txtCodigo, txtEmision, txtUsuario, txtNombreUsuario,
                        txtEstatus, txtComentario, txtTransporte, txtNombreTransporte, txtChoferTransporte, txtCapacidadTransporte,
                        txtFacturaDesde, txtFacturaHasta)

        ft.habilitarObjetos(False, True, txtTotal)

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

        ft.Ejecutar_strSQL(myconn, " delete from jsvenrenguipedidos where codigoguia = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")

    End Sub
    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        If Validado() Then
            GuardarTXT()
        End If
    End Sub
    Private Function Validado() As Boolean


        If txtNombreTransporte.Text = "" Then
            ft.MensajeCritico("Debe indicar un Transporte v�lido...")
            Return False
        End If


        If dtRenglones.Rows.Count = 0 Then
            ft.MensajeCritico("Debe incluir al menos un �tem...")
            Return False
        End If


        'If Not CBool(ParametroPlus(MyConn, Gestion.iVentas, "VENPARAM16")) Then
        '    If ValorCantidad(txtCapacidadTransporte.Text) < ValorCantidad(tslblPesoT.Text) Then
        '        ft.MensajeCritico( " LA GUIA EXCEDE LA CAPACIDAD DEL TRANSPORTE ... ")
        '    End If
        'End If

        Return True

    End Function
    Private Sub GuardarTXT()

        Dim Codigo As String = txtCodigo.Text
        Dim Inserta As Boolean = False
        If i_modo = movimiento.iAgregar Then
            Inserta = True
            nPosicionEncab = ds.Tables(nTabla).Rows.Count
            Codigo = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMGUIPED", "18")

            ft.Ejecutar_strSQL(myconn, " update jsvenrenguipedidos set codigoguia = '" & Codigo & "' where codigoguia = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")

        End If

        InsertEditVENTASEncabezadoGuiaPedidos(myConn, lblInfo, Inserta, Codigo, txtComentario.Text, jytsistema.sUsuario, txtTransporte.Text,
                                               txtEmision.Value, txtFacturaDesde.Value, txtFacturaHasta.Value,
                                               dtRenglones.Rows.Count, CDbl(txtTotal.Text), CDbl(tslblPesoT.Text), 0, 0)

        ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
        dt = ds.Tables(nTabla)
        Me.BindingContext(ds, nTabla).Position = nPosicionEncab
        ActualizarMovimientos(Codigo)
        AsignaTXT(nPosicionEncab)
        DesactivarMarco0()
        ft.ActivarMenuBarra(myConn, ds, dt, lRegion, MenuBarra, jytsistema.sUsuario)

    End Sub
    Private Sub ActualizarMovimientos(ByVal Codigo As String)
        Dim iCont As Integer

        ft.Ejecutar_strSQL(myconn, "UPDATE jsvenencped SET " _
                & " NUMPAG = '' WHERE NUMPAG = '" & Codigo & "' and " _
                & " EJERCICIO = '" & jytsistema.WorkExercise & "' and " _
                & " ID_EMP = '" & jytsistema.WorkID & "'")

        For iCont = 0 To dtRenglones.Rows.Count - 1
            With dtRenglones.Rows(iCont)
                ft.Ejecutar_strSQL(myconn, " update jsvenencped set NUMPAG = '" & Codigo & "' where NUMPED = '" & .Item("codigofac").ToString & "' and id_emp = '" & jytsistema.WorkID & "' ")
            End With
        Next
    End Sub
    Private Sub txtNombre_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtComentario.GotFocus
        ft.mensajeEtiqueta(lblInfo, " Indique comentario ... ", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub btnAgregar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregar.Click
        i_modo = movimiento.iAgregar
        nPosicionEncab = Me.BindingContext(ds, nTabla).Position
        ActivarMarco0()
        IniciarDocumento(True)
    End Sub

    Private Sub btnEditar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditar.Click

        i_modo = movimiento.iEditar
        nPosicionEncab = Me.BindingContext(ds, nTabla).Position
        ActivarMarco0()

    End Sub

    Private Sub btnEliminar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEliminar.Click

        Dim sRespuesta As Microsoft.VisualBasic.MsgBoxResult
        nPosicionEncab = Me.BindingContext(ds, nTabla).Position

        sRespuesta = MsgBox(" � Est� seguro que desea eliminar registro ?", MsgBoxStyle.YesNo, sModulo & " Eliminar registro ... ")

        If sRespuesta = MsgBoxResult.Yes Then

            InsertarAuditoria(myConn, MovAud.iEliminar, sModulo, dt.Rows(nPosicionEncab).Item("CODIGOGUIA"))
            ft.Ejecutar_strSQL(myconn, " delete from jsvenencguipedidos where " _
                & " codigoguia = '" & txtCodigo.Text & "' AND " _
                & " ID_EMP = '" & jytsistema.WorkID & "'")

            ft.Ejecutar_strSQL(myconn, " delete from jsvenrenguipedidos where codigoguia = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' ")

            ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
            dt = ds.Tables(nTabla)
            If dt.Rows.Count - 1 < nPosicionEncab Then nPosicionEncab = dt.Rows.Count - 1
            AsignaTXT(nPosicionEncab)

        End If

    End Sub

    Private Sub btnBuscar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuscar.Click
        Dim f As New frmBuscar
        Dim Campos() As String = {"codigoguia", "descripcion", "fechaguia"}
        Dim Nombres() As String = {"C�digo Gu�a", "Comentario", "Fecha Emisi�n"}
        Dim Anchos() As Integer = {150, 400, 100}
        f.Buscar(dt, Campos, Nombres, Anchos, Me.BindingContext(ds, nTabla).Position, "Gu�as de Despacho...")
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


    Private Sub dg_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles dg.RowHeaderMouseClick,
       dg.CellMouseClick
        Me.BindingContext(ds, nTablaRenglones).Position = e.RowIndex
        MostrarItemsEnMenuBarra(MenuBarraRenglon, e.RowIndex, ds.Tables(nTablaRenglones).Rows.Count)
    End Sub

    Private Sub CalculaTotales()

        txtTotal.Text = ft.FormatoNumero(ft.DevuelveScalarDoble(myConn, " select sum(totalfac) from jsvenrenguipedidos where codigoguia = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' group by codigoguia "))
        tslblPesoT.Text = ft.FormatoCantidad(ft.DevuelveScalarDoble(myConn, " select sum(kilosfac) from jsvenrenguipedidos where codigoguia = '" & txtCodigo.Text & "' and id_emp = '" & jytsistema.WorkID & "' group by codigoguia "))

    End Sub

    Private Sub btnAgregarMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregarMovimiento.Click
        Dim f As New jsVenProGuiaPedidosMovimientos
        f.Apuntador = Me.BindingContext(ds, nTablaRenglones).Position
        f.Cargar(myConn, txtFacturaDesde.Value, txtFacturaHasta.Value, txtCodigo.Text)
        AsignarMovimientos(txtCodigo.Text)
        f = Nothing
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
                    InsertarAuditoria(myConn, MovAud.iEliminar, sModulo, .Item("codigofac"))
                    Dim aCamposDel() As String = {"codigoguia", "codigofac", "id_emp"}
                    Dim aStringsDel() As String = {txtCodigo.Text, .Item("codigofac"), jytsistema.WorkID}

                    ft.Ejecutar_strSQL(myconn, " update jsvenencped set numpag = '' where numped = '" & .Item("codigofac") & "' and id_emp = '" & jytsistema.WorkID & "' ")

                    nPosicionRenglon = EliminarRegistros(myConn, lblInfo, ds, nTablaRenglones, "jsvenrenguipedidos", strSQLMov, aCamposDel, aStringsDel,
                                                  Me.BindingContext(ds, nTablaRenglones).Position, True)


                    If dtRenglones.Rows.Count - 1 < nPosicionRenglon Then nPosicionRenglon = dtRenglones.Rows.Count - 1
                    AsignaMov(nPosicionRenglon, True)
                    CalculaTotales()

                End With
            End If
        End If

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

    Private Sub btnAsesor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTransporte.Click
        txtTransporte.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codtra codigo, nomtra descripcion from jsconctatra where id_emp = '" & jytsistema.WorkID & "'  order by 1 ", "Transportes",
                                               txtTransporte.Text)
    End Sub

    Private Sub txtUsuario_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtUsuario.TextChanged
        txtNombreUsuario.Text = ft.DevuelveScalarCadena(myConn, " select nombre from jsconctausu where id_user = '" & txtUsuario.Text & "' ")
    End Sub


    Private Sub txtTransporte_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTransporte.TextChanged

        Dim nTransporte As String = ft.DevuelveScalarCadena(myConn, " select nomtra from jsconctatra where codtra = '" & txtTransporte.Text _
                                                         & "' and id_emp = '" & jytsistema.WorkID & "' ")
        If nTransporte = "0" Then nTransporte = ""
        txtNombreTransporte.Text = nTransporte

        txtCapacidadTransporte.Text = ft.FormatoCantidad(ft.DevuelveScalarDoble(myConn, " select capacidad from jsconctatra where codtra = '" & txtTransporte.Text _
                                                         & "' and id_emp = '" & jytsistema.WorkID & "' "))
        txtChoferTransporte.Text = ft.DevuelveScalarCadena(myConn, " select chofer from jsconctatra where codtra = '" & txtTransporte.Text _
                                                         & "' and id_emp = '" & jytsistema.WorkID & "' ")

    End Sub


    Private Sub btnCliente_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        txtUsuario.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codcli codigo, nombre descripcion, disponible, elt(estatus+1, 'Activo', 'Bloqueado', 'Inactivo', 'Desincorporado') estatus from jsvencatcli where estatus < 3 and id_emp = '" & jytsistema.WorkID & "' order by 1 ", "Clientes",
                                            txtUsuario.Text)
    End Sub

    Private Sub btnRelacion_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRelacion.Click
        Dim f As New jsVenRepParametros
        f.Cargar(TipoCargaFormulario.iShowDialog, ReporteVentas.cGuiaPedidos, "Relaci�n PEDIDOS", , txtCodigo.Text)
        f.Dispose()
        f = Nothing
    End Sub

    Private Sub btnMercancias_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMercancias.Click
        Dim f As New jsVenRepParametros
        f.Cargar(TipoCargaFormulario.iShowDialog, ReporteVentas.cGuiaPedidosMercancias, "Relaci�n de mercanc�as Gu�a PEDIDOS", , txtCodigo.Text)
        f.Dispose()
        f = Nothing
    End Sub

    'Private Sub btnFacturas_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFacturas.Click
    '    Dim sRespuesta As Microsoft.VisualBasic.MsgBoxResult
    '    sRespuesta = MsgBox(" � Est� seguro que desea imprimir facturas de la gu�a de despacho N� " & txtCodigo.Text & " ?", MsgBoxStyle.YesNo, sModulo & " Impresi�n Gu�a Despacho ... ")

    '    If sRespuesta = MsgBoxResult.Yes Then
    '        If dtRenglones.Rows.Count > 0 Then
    '            'If ParametrosGeneralVentas(MyDB, 5) Then
    '            ' JSVENPROIMPFAC.Cargar(jytsistema.sUsuario, MyDB, rsRenglon)
    '            'Else
    '            '   rsRenglon.MoveFirst()
    '            jytsistema.WorkBox = Registry.GetValue(jytsistema.DirReg, jytsistema.ClaveImpresorFiscal, "")
    '            If jytsistema.WorkBox = "" Then
    '                ft.MensajeCritico( "DEBE INDICAR UNA FORMA DE IMPRESION FISCAL. CONSULTE CON EL ADMINISTRADOR ...")
    '            Else
    '                Dim kCont As Integer
    '                For kCont = 0 To dtRenglones.Rows.Count - 1
    '                    'MsgBox(" imprimiendo guia  " & dtRenglones.Rows(kCont).Item("CODIGOFAC"), MsgBoxStyle.Information)
    '                    Imprimir(dtRenglones.Rows(kCont).Item("CODIGOFAC"))
    '                Next
    '                InsertarAuditoria(myConn, MovAud.iImprimir, sModulo, txtCodigo.Text & " Facturas gu�a ")
    '                'End If
    '            End If
    '        End If
    '    End If

    'End Sub


End Class
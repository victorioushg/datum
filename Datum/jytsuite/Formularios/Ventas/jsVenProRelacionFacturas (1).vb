Imports MySql.Data.MySqlClient
Public Class jsVenProRelacionFacturas
    Private Const sModulo As String = "Relacion De Facturas"
    Private Const nTabla As String = "tblEncabRelacionFacturas"
    Private Const lRegion As String = "RibbonButton220"
    Private Const nTablaRenglones As String = "tblRenglones_RelacionFacturas"

    Private strSQL As String = "select * from jsvenencrel where tipo = 0 and ejercicio = '" & JytSistema.WorkExercise & "' and ID_EMP = '" & JytSistema.WorkID & "' order by codigoguia "

    Private strSQLMov As String = ""

    Private myConn As New MySqlConnection(jytsistema.strConn)
    Private ds As New DataSet()
    Private dt As New DataTable
    Private dtRenglones As New DataTable

    Private i_modo As Integer
    Private nPosicionEncab As Long, nPosicionRenglon As Long

    Private aEstatus() As String = {"Activa", "Procesada"}

    Private Sub jsVenProRelacionFacturas_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Me.Dock = DockStyle.Fill
        Me.Tag = sModulo
        Try
            myConn.Open()

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
            ActivarMenuBarra(myConn, lblInfo, lRegion, ds, dt, MenuBarra)
            AsignarTooltips()

        Catch ex As MySql.Data.MySqlClient.MySqlException
            MensajeCritico(lblInfo, "Error en conexi�n de base de datos: " & ex.Message)
        End Try

    End Sub
    Private Sub AsignarTooltips()
        'Menu Barra 
        C1SuperTooltip1.SetToolTip(btnAgregar, "<B>Agregar</B> nueva Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnEditar, "<B>Editar o mofificar</B> Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnEliminar, "<B>Eliminar</B> Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnBuscar, "<B>Buscar</B> Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnPrimero, "ir a la <B>primer</B> Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnSiguiente, "ir a Relaci�n de Facturas <B>siguiente</B>")
        C1SuperTooltip1.SetToolTip(btnAnterior, "ir a Relaci�n de Facturas <B>anterior</B>")
        C1SuperTooltip1.SetToolTip(btnUltimo, "ir a la <B>�ltimo Relaci�n de Facturas</B>")

        C1SuperTooltip1.SetToolTip(btnSalir, "<B>Cerrar</B> esta ventana")

        'Menu barra rengl�n
        C1SuperTooltip1.SetToolTip(btnAgregarMovimiento, "<B>Agregar</B> rengl�n en Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnEditarMovimiento, "<B>Editar</B> rengl�n en Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnEliminarMovimiento, "<B>Eliminar</B> rengl�n en Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnBuscarMovimiento, "<B>Buscar</B> un rengl�n en Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnPrimerMovimiento, "ir al <B>primer</B> rengl�n en Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnAnteriorMovimiento, "ir al <B>anterior</B> rengl�n en Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnSiguienteMovimiento, "ir al rengl�n <B>siguiente </B> en Relaci�n de Facturas")
        C1SuperTooltip1.SetToolTip(btnUltimoMovimiento, "ir al <B>�ltimo</B> rengl�n de la Relaci�n de Facturas")

    End Sub

    Private Sub AsignaMov(ByVal nRow As Long, ByVal Actualiza As Boolean)
        Dim c As Integer = CInt(nRow)
        If Actualiza Then ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
        dtRenglones = ds.Tables(nTablaRenglones)
        If c >= 0 Then
            Me.BindingContext(ds, nTablaRenglones).Position = c
            MostrarItemsEnMenuBarra(MenuBarraRenglon, "itemsrenglon", "lblitemsrenglon", c, dtRenglones.Rows.Count)
            dg.Refresh()
            dg.CurrentCell = dg(0, c)
        End If

        ActivarMenuBarra(myConn, lblInfo, lRegion, ds, dtRenglones, MenuBarraRenglon)

    End Sub
    Private Sub AsignaTXT(ByVal nRow As Long)

        With dt

            nPosicionEncab = nRow
            Me.BindingContext(ds, nTabla).Position = nRow

            MostrarItemsEnMenuBarra(MenuBarra, "items", "lblitems", nRow, .Rows.Count)

            If nRow >= 0 Then
                With .Rows(nRow)
                    'Encabezado 
                    txtCodigo.Text = .Item("codigoguia")
                    txtEmision.Text = FormatoFecha(CDate(.Item("fechaguia").ToString))
                    txtUsuario.Text = .Item("elaborador")
                    txtEstatus.Text = aEstatus(.Item("estatus"))
                    txtComentario.Text = MuestraCampoTexto(.Item("descripcion"))
                    txtReponsable.Text = .Item("responsable")

                    tslblPesoT.Text = FormatoCantidad(.Item("totalkilos"))
                    txtTotal.Text = FormatoNumero(.Item("totalguia"))

                    txtFacturaDesde.Text = FormatoFecha(CDate(.Item("emisionfac").ToString))
                    txtFacturaHasta.Text = FormatoFecha(CDate(.Item("hastafac").ToString))

                    'Renglones
                    AsignarMovimientos(.Item("codigoguia"))

                    'Totales
                    CalculaTotales()

                End With
            End If

        End With
    End Sub
    Private Sub AsignarMovimientos(ByVal CodigoGuia As String)

        strSQLMov = "select *  " _
                    & " from jsvenrenrel " _
                    & " where CODIGOGUIA = '" & CodigoGuia & "' and " _
                    & " tipo = 0 and " _
                    & " ID_EMP = '" & jytsistema.WorkID & "' and " _
                    & " EJERCICIO = '" & jytsistema.WorkExercise & "' " _
                    & " order by CODVEN, CODIGOFAC "

        ds = DataSetRequery(ds, strSQLMov, myConn, nTablaRenglones, lblInfo)
        dtRenglones = ds.Tables(nTablaRenglones)

        Dim aCampos() As String = {"codigofac", "nomcli", "totalfac", "kilosfac", ""}
        Dim aNombres() As String = {"N�mero Factura", "Nombre Cliente", "Total Factura", "Peso Factura (Kgs)", ""}
        Dim aAnchos() As Long = {150, 470, 120, 120, 100}
        Dim aAlineacion() As Integer = {AlineacionDataGrid.Izquierda, AlineacionDataGrid.Izquierda, _
                                       AlineacionDataGrid.Derecha, AlineacionDataGrid.Derecha, _
                                        AlineacionDataGrid.Izquierda}

        Dim aFormatos() As String = {"", "", sFormatoNumero, sFormatoCantidad, ""}
        IniciarTabla(dg, dtRenglones, aCampos, aNombres, aAnchos, aAlineacion, aFormatos)
        If dtRenglones.Rows.Count > 0 Then
            nPosicionRenglon = 0
            AsignaMov(nPosicionRenglon, True)
        End If

    End Sub


    Private Sub IniciarDocumento(ByVal Inicio As Boolean)

        If Inicio Then
            txtCodigo.Text = "RELTMP" & NumeroAleatorio(100000)
        Else
            txtCodigo.Text = ""
        End If


        txtUsuario.Text = ""
        txtNombreUsuario.Text = ""
        txtComentario.Text = ""
        txtReponsable.Text = ""
        txtEstatus.Text = aEstatus(0)
        txtEmision.Text = FormatoFecha(sFechadeTrabajo)
        tslblPesoT.Text = FormatoCantidad(0)

        txtTotal.Text = FormatoNumero(0.0)
        txtFacturaDesde.Text = FormatoFecha(jytsistema.sFechadeTrabajo)
        txtFacturaHasta.Text = FormatoFecha(jytsistema.sFechadeTrabajo)


        'Movimientos
        MostrarItemsEnMenuBarra(MenuBarraRenglon, "itemsrenglon", "lblitemsrenglon", 0, 0)
        AsignarMovimientos(txtCodigo.Text)


    End Sub
    Private Sub ActivarMarco0()

        grpAceptarSalir.Visible = True

        HabilitarObjetos(True, False, grpEncab, grpTotales, MenuBarraRenglon)
        HabilitarObjetos(True, True, txtComentario, txtReponsable, btnEmision, btnDesde, btnHasta)
        MenuBarra.Enabled = False
        MensajeEtiqueta(lblInfo, "Haga click sobre cualquier bot�n de la barra menu...", TipoMensaje.iAyuda)

    End Sub
    Private Sub DesactivarMarco0()

        grpAceptarSalir.Visible = False
        HabilitarObjetos(False, True, txtCodigo, txtEmision, btnEmision, txtUsuario, txtNombreUsuario, txtReponsable, _
                        btnDesde, btnHasta, txtEstatus, _
                        txtComentario, txtFacturaDesde, txtFacturaHasta)

        HabilitarObjetos(False, True, txtTotal)

        MenuBarraRenglon.Enabled = False
        MenuBarra.Enabled = True

        MensajeEtiqueta(lblInfo, "Haga click sobre cualquier bot�n de la barra menu...", TipoMensaje.iAyuda)
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click

        If i_modo = movimiento.iAgregar Then AgregaYCancela()

        If dt.Rows.Count = 0 Then
            IniciarDocumento(False)
        Else
            Me.BindingContext(ds, nTabla).CancelCurrentEdit()
            AsignaTXT(nPosicionEncab)
        End If

        DesactivarMarco0()
    End Sub
    Private Sub AgregaYCancela()

        EjecutarSTRSQL(myConn, lblInfo, " delete from jsvenrenrel where codigoguia = '" & txtCodigo.Text & "' and tipo = 0 and id_emp = '" & jytsistema.WorkID & "' ")

    End Sub
    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        If Validado() Then
            GuardarTXT()
        End If
    End Sub
    Private Function Validado() As Boolean



        If dtRenglones.Rows.Count = 0 Then
            MensajeCritico(lblInfo, "Debe incluir al menos un �tem...")
            Return False
        End If


        Return True

    End Function
    Private Sub GuardarTXT()

        Dim Codigo As String = txtCodigo.Text
        Dim Inserta As Boolean = False
        If i_modo = movimiento.iAgregar Then
            Inserta = True
            nPosicionEncab = ds.Tables(nTabla).Rows.Count
            Codigo = Contador(myConn, lblInfo, Gestion.iVentas, "VENNUMREL", "16")

            EjecutarSTRSQL(myConn, lblInfo, " update jsvenrenrel set codigoguia = '" & Codigo & "' where codigoguia = '" & txtCodigo.Text & "' and tipo = '0' and  id_emp = '" & jytsistema.WorkID & "' ")
        End If

        InsertEditVENTASEncabezadoRelacionDeFacturas(myConn, lblInfo, Inserta, Codigo, txtComentario.Text, jytsistema.sUsuario, txtReponsable.Text, _
                                               CDate(txtEmision.Text), CDate(txtFacturaDesde.Text), CDate(txtFacturaHasta.Text), _
                                               dtRenglones.Rows.Count, CDbl(txtTotal.Text), CDbl(tslblPesoT.Text), 0, 0, 0)

        EjecutarSTRSQL(myConn, lblInfo, " UPDATE jsvenencfac a, jsvenrenrel b " _
                       & " SET a.relfacturas = b.codigoguia " _
                       & " WHERE " _
                       & " b.tipo = '0' AND " _
                       & " b.codigoguia = '" & Codigo & "' AND " _
                       & " a.id_emp = '" & jytsistema.WorkID & "' AND " _
                       & " a.emision = b.emision AND " _
                       & " a.numfac = b.codigofac AND " _
                       & " a.id_emp = b.id_emp ")

        EjecutarSTRSQL(myConn, lblInfo, " UPDATE jsvenencncr a, jsvenrenrel b " _
                       & " SET a.relfacturas = b.codigoguia " _
                       & " WHERE " _
                       & " b.tipo = '0' AND " _
                       & " b.codigoguia = '" & Codigo & "' AND " _
                       & " a.id_emp = '" & jytsistema.WorkID & "' AND " _
                       & " a.emision = b.emision AND " _
                       & " a.numncr = b.codigofac AND " _
                       & " a.id_emp = b.id_emp ")

        EjecutarSTRSQL(myConn, lblInfo, " UPDATE jsvenencndb a, jsvenrenrel b " _
                       & " SET a.relfacturas = b.codigoguia " _
                       & " WHERE " _
                       & " b.tipo = '0' AND " _
                       & " b.codigoguia = '" & Codigo & "' AND " _
                       & " a.id_emp = '" & jytsistema.WorkID & "' AND " _
                       & " a.emision = b.emision AND " _
                       & " a.numndb = b.codigofac AND " _
                       & " a.id_emp = b.id_emp ")


        ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
        dt = ds.Tables(nTabla)

        Dim row As DataRow = dt.Select(" CODIGOGUIA = '" & Codigo & "' AND TIPO = 0 AND ID_EMP = '" & jytsistema.WorkID & "' ")(0)
        nPosicionEncab = dt.Rows.IndexOf(row)
        Me.BindingContext(ds, nTabla).Position = nPosicionEncab

        AsignaTXT(nPosicionEncab)
        DesactivarMarco0()
        ActivarMenuBarra(myConn, lblInfo, lRegion, ds, dt, MenuBarra)

    End Sub

    Private Sub txtNombre_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtComentario.GotFocus
        MensajeEtiqueta(lblInfo, " Indique comentario ... ", TipoMensaje.iInfo)
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

        sRespuesta = MsgBox(" � Esta Seguro que desea eliminar registro ?", MsgBoxStyle.YesNo, sModulo & " Eliminar registro ... ")

        If sRespuesta = MsgBoxResult.Yes Then

            InsertarAuditoria(myConn, MovAud.iEliminar, sModulo, dt.Rows(nPosicionEncab).Item("CODIGOGUIA"))
            EjecutarSTRSQL(myConn, lblInfo, " delete from jsvenencrel where " _
                & " codigoguia = '" & txtCodigo.Text & "' AND " _
                & " tipo = '0' and " _
                & " ID_EMP = '" & jytsistema.WorkID & "'")

            EjecutarSTRSQL(myConn, lblInfo, " delete from jsvenrenrel where codigoguia = '" & txtCodigo.Text & "' and  tipo = 0 and id_emp = '" & jytsistema.WorkID & "' ")

            ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
            dt = ds.Tables(nTabla)
            If dt.Rows.Count - 1 < nPosicionEncab Then nPosicionEncab = dt.Rows.Count - 1
            AsignaTXT(nPosicionEncab)

        End If

    End Sub

    Private Sub btnBuscar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuscar.Click
        Dim f As New frmBuscar
        Dim Campos() As String = {"codigoguia", "descripcion", "fechaguia"}
        Dim Nombres() As String = {"C�digo Relaci�n", "Comentario", "Fecha Emisi�n"}
        Dim Anchos() As Long = {150, 400, 100}
        f.Buscar(dt, Campos, Nombres, Anchos, Me.BindingContext(ds, nTabla).Position, "Relaci�n de facturas...")
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
        MostrarItemsEnMenuBarra(MenuBarraRenglon, "ItemsRenglon", "lblItemsRenglon", e.RowIndex, ds.Tables(nTablaRenglones).Rows.Count)
    End Sub

    Private Sub CalculaTotales()

        txtTotal.Text = FormatoNumero(CDbl(EjecutarSTRSQL_Scalar(myConn, lblInfo, " select sum(totalfac) from jsvenrenrel where codigoguia = '" & txtCodigo.Text & "' and tipo = 0 and id_emp = '" & jytsistema.WorkID & "' group by codigoguia ")))
        tslblPesoT.Text = FormatoCantidad(CDbl(EjecutarSTRSQL_Scalar(myConn, lblInfo, " select sum(kilosfac) from jsvenrenrel where codigoguia = '" & txtCodigo.Text & "' and tipo = 0 and id_emp = '" & jytsistema.WorkID & "' group by codigoguia ")))

    End Sub

    Private Sub btnAgregarMovimiento_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregarMovimiento.Click
        Dim f As New jsVenProRelacionFacturasMovimientosPlus
        f.Apuntador = Me.BindingContext(ds, nTablaRenglones).Position
        f.Cargar(myConn, CDate(txtFacturaDesde.Text), CDate(txtFacturaHasta.Text), txtCodigo.Text)
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
            sRespuesta = MsgBox(" � Esta Seguro que desea eliminar registro ?", MsgBoxStyle.YesNo, "Eliminar registro ... ")
            If sRespuesta = MsgBoxResult.Yes Then
                With dtRenglones.Rows(nPosicionRenglon)
                    InsertarAuditoria(myConn, MovAud.iEliminar, sModulo, .Item("CODIGOFAC"))
                    Dim aCamposDel() As String = {"codigoguia", "codigofac", "tipo", "id_emp"}
                    Dim aStringsDel() As String = {txtCodigo.Text, .Item("codigofac"), .Item("tipo"), jytsistema.WorkID}

                    EjecutarSTRSQL(myConn, lblInfo, " update jsvenencfac set relfacturas = '' where " _
                                   & " RELFACTURAS = '" & txtCodigo.Text & "' AND numfac = '" & .Item("codigofac") & "' and id_emp = '" & jytsistema.WorkID & "' ")

                    nPosicionRenglon = EliminarRegistros(myConn, lblInfo, ds, nTablaRenglones, "jsvenrenrel", strSQLMov, aCamposDel, aStringsDel, _
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

    Private Sub btnEmision_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEmision.Click
        txtEmision.Text = SeleccionaFecha(CDate(txtEmision.Text), Me, sender)
    End Sub


    Private Sub txtUsuario_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtUsuario.TextChanged
        txtNombreUsuario.Text = EjecutarSTRSQL_Scalar(myConn, lblInfo, " select nombre from jsconctausu where id_user = '" & txtUsuario.Text & "' ")
    End Sub


    Private Sub btnCliente_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        txtUsuario.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codcli codigo, nombre descripcion, disponible, elt(estatus+1, 'Activo', 'Bloqueado', 'Inactivo', 'Desincorporado') estatus from jsvencatcli where estatus < 3 and id_emp = '" & jytsistema.WorkID & "' order by 1 ", "Clientes", _
                                            txtUsuario.Text)
    End Sub


    Private Sub btnDesde_Click(sender As System.Object, e As System.EventArgs) Handles btnDesde.Click
        txtFacturaDesde.Text = SeleccionaFecha(CDate(txtFacturaDesde.Text), Me, sender)
    End Sub

    Private Sub btnHasta_Click(sender As System.Object, e As System.EventArgs) Handles btnHasta.Click
        txtFacturaHasta.Text = SeleccionaFecha(CDate(txtFacturaHasta.Text), Me, sender)
    End Sub

    Private Sub btnImprimir_Click(sender As System.Object, e As System.EventArgs) Handles btnImprimir.Click
        Dim f As New jsVenRepParametros
        f.Cargar(TipoCargaFormulario.iShowDialog, ReporteVentas.cRelacionFacturas, "Relaci�n Facturas", , txtCodigo.Text)
        f.Dispose()
        f = Nothing
    End Sub

  
End Class
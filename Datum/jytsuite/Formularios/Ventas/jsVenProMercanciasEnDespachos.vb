Imports MySql.Data.MySqlClient
Public Class jsVenProMercanciasEnDespachos
    Private Const sModulo As String = "Mercanc�as en Despachos"
    Private Const nTabla As String = "tblMercanciasEnDespachos"
    Private Const lRegion As String = "RibbonButton272"


    Private strSQLMov As String = ""

    Private myConn As New MySqlConnection(jytsistema.strConn)
    Private ds As New DataSet()
    Private dtRenglones As New DataTable
    Private ft As New Transportables

    Private i_modo As Integer
    Private nPosicion As Long

    Private tblTemp As String = "tbl" & ft.NumeroAleatorio(100000)

    Private Sub jsVenProMercanciasEnDespachos_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ft = Nothing
    End Sub

    
    Private Sub jsVenProMercanciasEnDespachos_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Me.Dock = DockStyle.Fill
        Me.Tag = sModulo
        Try
            myConn.Open()



            ft.habilitarObjetos(False, True, txtAsesorDesde, txtAsesorHasta, txtFechaDesde, txtFechaHasta, txtMercanciaDesde, txtMercanciaHasta)
            txtFechaDesde.Text = ft.FormatoFecha(jytsistema.sFechadeTrabajo)
            txtFechaHasta.Text = ft.FormatoFecha(jytsistema.sFechadeTrabajo)


        Catch ex As MySql.Data.MySqlClient.MySqlException
            ft.MensajeCritico("Error en conexi�n de base de datos: " & ex.Message)
        End Try

    End Sub

    Private Sub AsignarMovimientos()

        Dim aFld() As String = {"numped.cadena.15.0", "item.cadena.15.0", "descrip.cadena.150.0", "cantidad.doble.10.3", "cantran.doble.10.3", "unidad.cadena.5.0", "emision.fecha.0.0", "codcli.cadena.15.0", "nombre.cadena.250.0", "renglon.cadena.5.0", "precio.doble.19.2", "des_art.doble.6.2", "des_cli.doble.6.2", "des_ofe.doble.6.2"}
        CrearTabla(myConn, lblInfo, jytsistema.WorkDataBase, True, tblTemp, aFld, " numped ")
        ft.Ejecutar_strSQL(myconn, " INSERT INTO " & tblTemp _
                        & " SELECT a.numped, a.item, a.descrip, a.cantidad, a.cantran, a.unidad, b.emision, b.codcli, c.nombre, a.renglon, a.precio, a.des_art, a.des_cli, a.des_ofe " _
                        & " FROM jsvenrenped a " _
                        & " LEFT JOIN jsvenencped b  ON (a.numped = b.numped AND a.id_emp = b.id_emp) " _
                        & " LEFT JOIN jsvencatcli c ON ( b.codcli = c.codcli AND b.id_emp = c.id_emp) " _
                        & " WHERE " _
                        & " a.item >= '" & txtMercanciaDesde.Text & "' AND  a.item <= '" & txtMercanciaHasta.Text & "' AND " _
                        & " b.codven >= '" & txtAsesorDesde.Text & "' AND b.codven <= '" & txtAsesorHasta.Text & "' AND " _
                        & " b.emision >= '" & ft.FormatoFechaMySQL(CDate(txtFechaDesde.Text)) & "' AND " _
                        & " b.emision <= '" & ft.FormatoFechaMySQL(CDate(txtFechaHasta.Text)) & "' AND " _
                        & " a.id_emp = '" & jytsistema.WorkID & "' ")

        strSQLMov = " SELECT * FROM " & tblTemp & " ORDER BY numped "

        ds = DataSetRequery(ds, strSQLMov, myConn, nTabla, lblInfo)
        dtRenglones = ds.Tables(nTabla)
        Dim aCampos() As String = {"numped", "item", "descrip", "cantran", "unidad", "emision", "codcli", "nombre"}
        Dim aNombres() As String = {"N�mero Factura", "Item", "Descripci�n", "Cantidad Tr�nsito", "Unidad", "Emisi�n", "Cliente", ""}
        Dim aAnchos() As Integer = {100, 100, 370, 100, 50, 100, 100, 300}
        Dim aAlineacion() As Integer = {AlineacionDataGrid.Izquierda, AlineacionDataGrid.Izquierda, _
                                       AlineacionDataGrid.Izquierda, AlineacionDataGrid.Derecha, _
                                        AlineacionDataGrid.Centro, AlineacionDataGrid.Centro, _
                                        AlineacionDataGrid.Izquierda, AlineacionDataGrid.Izquierda}

        Dim aFormatos() As String = {"", "", "", sFormatoCantidad, "", sFormatoFecha, "", ""}
        IniciarTabla(dg, dtRenglones, aCampos, aNombres, aAnchos, aAlineacion, aFormatos, , True)
        If dtRenglones.Rows.Count > 0 Then nPosicion = 0

        dg.ReadOnly = False
        dg.Columns("numped").ReadOnly = True
        dg.Columns("item").ReadOnly = True
        dg.Columns("descrip").ReadOnly = True
        dg.Columns("unidad").ReadOnly = True
        dg.Columns("emision").ReadOnly = True
        dg.Columns("codcli").ReadOnly = True
        dg.Columns("nombre").ReadOnly = True
        dg.Columns("cantran").ReadOnly = False


    End Sub



    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click

        Me.Close()

    End Sub
    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        If Validado() Then
            GuardarTXT()
        End If
    End Sub
    Private Function Validado() As Boolean

        If txtAsesorDesde.Text = "" Or txtAsesorHasta.Text = "" Then
            ft.MensajeCritico("Debe INDICAR un asesor v�lido...")
            Return False
        End If

        If txtMercanciaDesde.Text = "" Or txtMercanciaHasta.Text = "" Then
            ft.MensajeCritico("Debe INDICAR una mercanc�a v�lida...")
            Return False
        End If

        Return True

    End Function
    Private Sub GuardarTXT()


        dtRenglones.AcceptChanges()

        For dCont As Integer = 0 To dtRenglones.Rows.Count - 1

            With dtRenglones.Rows(dCont)
                '1.- ACTUALIZAR RENGLONES EN PEDIDO . OJO. DESCUENTOS POR RENGLON
                If .Item("cantran") > 0 Then
                    Dim TotalRenglon As Double = Math.Round(.Item("cantran") * .Item("precio") * (1 - .Item("des_art") / 100) * (1 - .Item("des_cli") / 100) * (1 - .Item("des_ofe") / 100), 2)

                    ft.Ejecutar_strSQL(myconn, " update jsvenrenped set cantidad = " & .Item("cantran") _
                                    & ", cantran = " & .Item("cantran") & ", totren = " & TotalRenglon & ", totrendes = " & TotalRenglon & " " _
                                    & " where " _
                                    & " numped = '" & .Item("numped") & "' and " _
                                    & " renglon = '" & .Item("renglon") & "' and " _
                                    & " item = '" & .Item("item") & "' and " _
                                    & " id_emp = '" & jytsistema.WorkID & "' ")
                Else
                    ft.Ejecutar_strSQL(myconn, " delete from  jsvenrenped " _
                                   & " where " _
                                   & " numped = '" & .Item("numped") & "' and " _
                                   & " renglon = '" & .Item("renglon") & "' and " _
                                   & " item = '" & .Item("item") & "' and " _
                                   & " id_emp = '" & jytsistema.WorkID & "' ")
                End If

                Dim SubtotalPedido As Double = CalculaTotalRenglonesVentas(myConn, lblInfo, "jsvenrenped", "numped", "totren", .Item("numped"), 0)
                Dim DescuentoGlobal As Double = ft.DevuelveScalarDoble(myConn, " select SUM(DESCUENTO) from jsvendesped where numped = '" & .Item("numped") & "' and id_emp = '" & jytsistema.WorkID & "' group by numped ")
                Dim Cargos As Double = CalculaTotalRenglonesVentas(myConn, lblInfo, "jsvenrenped", "numped", "totren", .Item("numped"), 1)

                ft.Ejecutar_strSQL(myConn, " update jsvenrenped set totrendes = totren - totren * " & DescuentoGlobal / IIf(SubtotalPedido > 0, SubtotalPedido, 1) & " " _
                    & " where " _
                    & " numped = '" & .Item("numped") & "' and " _
                    & " renglon > '' and " _
                    & " item > '' and " _
                    & " ESTATUS = '0' AND " _
                    & " ACEPTADO < '2' and " _
                    & " EJERCICIO = '" & jytsistema.WorkExercise & "' and " _
                    & " ID_EMP = '" & jytsistema.WorkID & "' ")

                CalculaTotalIVAVentas(myConn, lblInfo, "jsvendesped", "jsvenivaped", "jsvenrenped", "numped", .Item("numped"), "impiva", "totrendes", CDate(.Item("emision").ToString), "totren")
                Dim TotalIVA As Double = ft.DevuelveScalarDoble(myConn, " select SUM(IMPIVA) from jsvenivaped where numped = '" & .Item("numped") & "' and id_emp = '" & jytsistema.WorkID & "' group by numped ")

                Dim TotalPedido As Double = SubtotalPedido - DescuentoGlobal + Cargos + TotalIVA
                Dim TotalPesoPedido As Double = CalculaPesoDocumento(myConn, lblInfo, "jsvenrenped", "peso", "numped", .Item("numped"))

                'ACTUALIZA ENCABEZADO
                ft.Ejecutar_strSQL(myConn, " UPDATE jsvenencped set TOT_NET = " & SubtotalPedido & ", DESCUEN = " & DescuentoGlobal & ", CARGOS = " & Cargos & ", IMP_IVA = " & TotalIVA & " , TOT_PED = " & TotalPedido & ", KILOS = " & TotalPesoPedido & "  WHERE numped = '" & .Item("numped") & "' and id_emp = '" & jytsistema.WorkID & "'  ")


            End With


        Next


    End Sub

    Private Sub txtNombre_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtFechaHasta.GotFocus
        ft.mensajeEtiqueta(lblInfo, " Indique comentario ... ", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub btnFechaDesde_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFechaDesde.Click
        txtFechaDesde.Text = SeleccionaFecha(CDate(txtFechaDesde.Text), Me, grpEncab, btnFechaDesde)
    End Sub

    Private Sub btnFechaHasta_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFechaHasta.Click
        txtFechaHasta.Text = SeleccionaFecha(CDate(txtFechaHasta.Text), Me, grpEncab, btnFechaHasta)
    End Sub

    Private Sub btnMercanciaDesde_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMercanciaDesde.Click
        txtMercanciaDesde.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codart codigo, nomart descripcion from jsmerctainv where id_emp = '" & jytsistema.WorkID & "' order by codart ", "Mercanc�as", txtMercanciaDesde.Text)
    End Sub

    Private Sub btnMercanciaHasta_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMercanciaHasta.Click
        txtMercanciaHasta.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codart codigo, nomart descripcion from jsmerctainv where id_emp = '" & jytsistema.WorkID & "' order by codart ", "Mercanc�as", txtMercanciaHasta.Text)
    End Sub

    Private Sub btnAsesorDesde_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAsesorDesde.Click
        txtAsesorDesde.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codven codigo, concat(apellidos, ' ', nombres) descripcion from jsvencatven where tipo = 0 and id_emp = '" & jytsistema.WorkID & "' order by codven ", "Asesores Comerciales", txtAsesorDesde.Text)
    End Sub

    Private Sub btnAsesorHasta_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAsesorHasta.Click
        txtAsesorHasta.Text = CargarTablaSimple(myConn, lblInfo, ds, " select codven codigo, concat(apellidos, ' ', nombres) descripcion from jsvencatven where tipo = 0 and id_emp = '" & jytsistema.WorkID & "' order by codven ", "Asesores Comerciales", txtAsesorHasta.Text)
    End Sub

    Private Sub txtFechaDesde_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFechaDesde.TextChanged
        txtFechaHasta.Text = txtFechaDesde.Text
    End Sub

    Private Sub txtMercanciaDesde_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMercanciaDesde.TextChanged
        txtMercanciaHasta.Text = txtMercanciaDesde.Text
    End Sub

    Private Sub txtAsesorDesde_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAsesorDesde.TextChanged
        txtAsesorHasta.Text = txtAsesorDesde.Text
    End Sub

    Private Sub btnGo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGo.Click
        If Validado() Then
            AsignarMovimientos()
        End If
    End Sub
    Private Sub dg_CancelRowEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.QuestionEventArgs) Handles dg.CancelRowEdit
        ft.mensajeAdvertencia("CANCELANDO...")
    End Sub
    Private Sub dg_CellValidating(ByVal sender As Object, ByVal e As DataGridViewCellValidatingEventArgs) _
            Handles dg.CellValidating

        If e.ColumnIndex = 3 Then
            If (String.IsNullOrEmpty(e.FormattedValue.ToString())) Then
                ft.MensajeCritico("Debe indicar d�gito(s) v�lido...")
                e.Cancel = True
            End If

            If Not ft.isNumeric(e.FormattedValue.ToString()) Then
                ft.mensajeCritico("Debe indicar un n�mero v�lido...")
                e.Cancel = True
            End If
        End If

    End Sub
  
End Class
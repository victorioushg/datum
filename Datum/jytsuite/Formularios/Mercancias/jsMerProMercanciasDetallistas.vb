Imports MySql.Data.MySqlClient
Public Class jsMerProMercanciasDetallistas

    Private Const sModulo As String = "Existenbcias para detallistas"
    Private Const lRegion As String = ""
    Private Const nTabla As String = "tblExisteAlmacen"

    Private strSQL As String = "SELECT a.codart, a.nomart, a.alterno, a.barras, a.grupo, a.marca, a.division, " _
                               & " a.tipjer, a.presentacion, a.iva, a.ubicacion, a.unidaddetal, " _
                               & " ROUND(a.pesounidad/if(b.equivale is null, 1, b.equivale), 3) pesodetal, a.estatus, b.equivale " _
                               & " " _
                               & " FROM jsmerctainv a " _
                               & " LEFT JOIN jsmerequmer b ON (a.codart = b.codart AND a.unidad = b.unidad AND a.unidaddetal = b.uvalencia AND a.id_emp = b.id_emp ) " _
                               & " WHERE " _
                               & " a.id_emp = '" & jytsistema.WorkID & "' ORDER BY codart "

    Private strSQLMov As String

    Private myConn As New MySqlConnection(jytsistema.strConn)
    Private myCom As New MySqlCommand(strSQL, myConn)
    Private ds As New DataSet()
    Private dt As New DataTable
    Private ft As New Transportables

    Private i_modo As Integer
    Private nPosicion As Long
    Private aCondicion() As String = {"Activo", "Inactivo"}
    Private iMeses As Integer = 3
    Private iSugerido As Integer = 10
    Private nEquivale As Double = 0.0
    Private nUND As String = ""
    Private AlmacenDetal As String = ""

    Private Sub jsMerProMercanciasDetallistas_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ft = Nothing
    End Sub

    Private Sub jsMerProMercanciasDetallistas_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Me.Dock = DockStyle.Fill

        Try
            myConn.Open()

            ds = DataSetRequeryPlus(ds, nTabla, myConn, strSQL)
            dt = ds.Tables(nTabla)


            AlmacenDetal = Convert.ToString(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM20"))

            ft.RellenaCombo(aCondicion, cmbCondicion)
            ft.RellenaCombo(aUnidad, cmbUnidad)

            DesactivarMarco0()
            If dt.Rows.Count > 0 Then
                nPosicion = 0
                Me.BindingContext(ds, nTabla).Position = nPosicion
                AsignaTXT(nPosicion)
            End If
            ft.ActivarMenuBarra(myConn, ds, dt, lRegion, MenuBarra, jytsistema.sUsuario)
            AsignarTooltips()

        Catch ex As MySql.Data.MySqlClient.MySqlException
            ft.MensajeCritico("Error en conexi�n de base de datos: " & ex.Message)
        End Try

    End Sub
    Private Sub AsignarTooltips()
        'Menu Barra 
        C1SuperTooltip1.SetToolTip(btnAgregar, "<B>Agregar</B> nuevo registro")
        C1SuperTooltip1.SetToolTip(btnEditar, "<B>Editar o mofificar</B> registro actual")
        C1SuperTooltip1.SetToolTip(btnEliminar, "<B>Eliminar</B> registro actual")
        C1SuperTooltip1.SetToolTip(btnBuscar, "<B>Buscar</B> registro deseada")
        C1SuperTooltip1.SetToolTip(btnPrimero, "ir a la <B>primera</B> registro")
        C1SuperTooltip1.SetToolTip(btnSiguiente, "ir a registro <B>siguiente</B>")
        C1SuperTooltip1.SetToolTip(btnAnterior, "ir a registro <B>anterior</B>")
        C1SuperTooltip1.SetToolTip(btnUltimo, "ir a la <B>�ltima registro</B>")
        C1SuperTooltip1.SetToolTip(btnImprimir, "<B>Imprimir</B> registro actual")
        C1SuperTooltip1.SetToolTip(btnSalir, "<B>Cerrar</B> esta ventana")
    End Sub


    Private Sub AsignaTXT(ByVal nRow As Long)

        With dt

            nPosicion = nRow
            Me.BindingContext(ds, nTabla).Position = nRow

            MostrarItemsEnMenuBarra(MenuBarra, nRow, .Rows.Count)

            With .Rows(nRow)
                'Encabezado 
                txtCodigo.Text = .Item("codart")
                txtAlterno.Text = ft.muestraCampoTexto(.Item("alterno"))
                txtBarras.Text = ft.muestraCampoTexto(.Item("barras"))
                txtDescripcion.Text = ft.muestraCampoTexto(.Item("nomart"))
                txtCategoria.Text = ft.muestraCampoTexto(.Item("grupo"))
                txtMarca.Text = ft.muestraCampoTexto(.Item("marca"))
                txtDivision.Text = ft.muestraCampoTexto(.Item("division"))
                txtJerarquia.Text = ft.muestraCampoTexto(.Item("tipjer"))
                txtPresentacion.Text = ft.muestraCampoTexto(.Item("presentacion"))

                Dim aIVA() As String = ArregloIVA(myConn, lblInfo)
                ft.RellenaCombo(aIVA, cmbIVA, ft.InArray(aIVA, .Item("iva")))

                Dim aUbica() As String = Split(.Item("ubicacion"), ".")
                txtUbica1.Text = ""
                txtUbica2.Text = ""
                txtUbica3.Text = ""
                If aUbica.Length = 1 Then txtUbica1.Text = aUbica(0)
                If aUbica.Length = 2 Then txtUbica2.Text = aUbica(1)
                If aUbica.Length = 3 Then txtUbica3.Text = aUbica(2)

                cmbCondicion.SelectedIndex = .Item("estatus")

                cmbUnidad.SelectedIndex = ft.InArray(aUnidadAbreviada, .Item("unidaddetal"))
                txtPesoUnidad.Text = IIf(IsDBNull(.Item("pesodetal")), ft.FormatoCantidad(0.0), ft.FormatoCantidad(.Item("pesodetal")))
                nEquivale = IIf(IsDBNull(.Item("equivale")), 1, .Item("equivale"))
                nUND = .Item("UNIDADDETAL")

                txtSugeridoDias.Text = ft.FormatoEntero(iSugerido)
                txtPromedioMeses.Text = ft.FormatoEntero(iMeses)

                ft.habilitarObjetos(False, True, txtExisteUnidad, txtPromedioUnidad, txtSugeridoUnidad, _
                                 txtExistenciaKilos, txtPromedioKilos, txtSugeridoKilos, txtInventarioDias, _
                                 txtUnidad1, txtUnidad2, txtUnidad4, txtKilos1, txtKilos2, txtKilos4)

                ExistenciasPor(.Item("codart"), 0, iMeses, iSugerido)

                CargarPrecios(.Item("codart"))

            End With
        End With
    End Sub

    Private Sub ExistenciasPor(ByVal nArticulo As String, ByVal por As Integer, ByVal nMeses As Integer, nDiasSugerido As Integer)

        Dim dtExPor As DataTable
        Dim nTablaExPor As String = "tablaexPor"


        Dim str1 As String = ""
        Dim str2 As String = ""
        Dim str3 As String = ""
        Dim str4 As String = ""
        Select Case por
            Case 0 ' Por almacen 
                str1 = " a.almacen codigo, e.desalm descripcion, "
                str2 = " b.almacen, "
                str3 = " left join jsmercatalm e ON (a.almacen = e.codalm AND a.id_emp = e.id_emp )  "
                str4 = " GROUP BY 1,2 "
            Case 1 'Por LOTE
                str1 = " a.lote codigo, e.expiracion descripcion,  "
                str2 = " b.lote, "
                str3 = " left join jsmerlotmer e on (a.lote = e.lote and a.id_emp = e.id_emp ) "
                str4 = " group by 1,2 "
            Case Else
        End Select

        ds = DataSetRequeryPlus(ds, nTablaExPor, myConn, " SELECT a.codart, " & str1 & " '" & nUND & "' unidad, round(SUM(IF( ISNULL(f.uvalencia), a.existencia, a.existencia/f.equivale))*" & nEquivale & ", 3)  existencia , " _
            & " round(c.pesounidad*SUM(IF( ISNULL(f.uvalencia), a.existencia, a.existencia/f.equivale)),3) pesototal, " _
            & " ROUND(SUM(IF( ISNULL(f.uvalencia), a.ventas, a.ventas/f.equivale)),3) ventasperiodo,  d.diashabiles, " _
            & " ROUND(SUM(IF( ISNULL(f.uvalencia), a.ventas, a.ventas/f.equivale)) / d.DiasHabiles, 3) promedioDiario,  " _
            & " ROUND(SUM(IF( ISNULL(f.uvalencia), a.ventas, a.ventas/f.equivale)) / d.DiasHabiles * c.pesounidad,3) promedioDiarioPeso , " _
            & " SUM(IF( ISNULL(f.uvalencia), a.existencia, a.existencia/f.equivale)) / (  SUM(IF( ISNULL(f.uvalencia), a.ventas, a.ventas/f.equivale)) / d.DiasHabiles ) Inventario " _
            & " FROM (SELECT  b.codart, " & str2 & " b.unidad,  " _
            & " SUM(IF( b.TIPOMOV IN( 'EN', 'AE', 'DV') , b.CANTIDAD, -1 * b.CANTIDAD )) existencia, " _
            & " SUM(IF( b.origen IN ('FAC', 'PVE', 'PFC') AND b.fechamov >= DATE_ADD('" & ft.FormatoFechaMySQL(jytsistema.sFechadeTrabajo) & "', INTERVAL -" & nMeses & " MONTH) AND  b.fechamov <= DATE_ADD('" & ft.FormatoFechaMySQL(jytsistema.sFechadeTrabajo) & "', INTERVAL 1 DAY) ,  b.cantidad, 0.000 ) ) ventas, " _
            & " " _
            & " b.id_emp " _
            & " FROM jsmertramer b " _
            & " WHERE " _
            & "      b.almacen = '" & AlmacenDetal & "' AND " _
            & "      b.tipomov <> 'AC' AND " _
            & "      b.codart = '" & nArticulo & "' AND " _
            & "      b.id_emp = '" & jytsistema.WorkID & "' AND  " _
            & "      b.ejercicio = '" & jytsistema.WorkExercise & "' AND  " _
            & "      date_format(b.fechamov, '%Y-%m-%d') <= '" & ft.FormatoFechaMySQL(jytsistema.sFechadeTrabajo) & "' " _
            & "      GROUP BY b.codart, " & str2 & " b.unidad ) a  " _
            & " LEFT JOIN jsmerequmer f ON (a.codart = f.codart AND a.unidad = f.uvalencia AND a.id_emp = f.id_emp )  " _
            & " LEFT JOIN jsmerctainv c ON (a.codart = c.codart AND a.id_emp = c.id_emp) " _
            & " LEFT JOIN (SELECT '1' num, (CURRENT_DATE - DATE_ADD(CURRENT_DATE, INTERVAL -" & nMeses & " MONTH)) -  IFNULL(COUNT(*),0) DiasHabiles " _
            & " FROM (SELECT CONCAT(ano,'-',IF(LENGTH(mes)=1,CONCAT('0',mes),mes),'-',IF(LENGTH(dia)=1,CONCAT('0',dia),dia)) AS fecha FROM jsconcatper WHERE MODULO = 1 AND ID_EMP = '" & jytsistema.WorkID & "'  " _
            & " HAVING  (fecha <CURRENT_DATE AND fecha>DATE_ADD(CURRENT_DATE,INTERVAL -" & nMeses & " MONTH)) )  a) d ON ( d.num = '1') " _
            & str3 _
            & str4)

        dtExPor = ds.Tables(nTablaExPor)

        If dtExPor.Rows.Count > 0 Then

            With dtExPor.Rows(0)
                txtExisteUnidad.Text = ft.FormatoCantidad(.Item("existencia"))
                txtExistenciaKilos.Text = ft.FormatoCantidad(.Item("pesototal"))
                txtInventarioDias.Text = ft.FormatoEntero(IIf(IsDBNull(.Item("inventario")), 0, .Item("inventario")))
                txtPromedioUnidad.Text = ft.FormatoCantidad(IIf(IsDBNull(.Item("promediodiario")), 0, .Item("promediodiario")))
                txtPromedioKilos.Text = ft.FormatoCantidad(IIf(IsDBNull(.Item("promediodiariopeso")), 0, .Item("promediodiariopeso")))
                txtSugeridoUnidad.Text = ft.FormatoCantidad(ValorEntero(txtSugeridoDias.Text) * IIf(IsDBNull(.Item("promediodiario")), 0, .Item("promediodiario")))
                txtSugeridoKilos.Text = ft.FormatoCantidad(ValorEntero(txtSugeridoDias.Text) * IIf(IsDBNull(.Item("promediodiariopeso")), 0, .Item("promediodiariopeso")))
                txtUnidad1.Text = nUND
                txtUnidad2.Text = nUND
                txtUnidad4.Text = nUND
                txtKilos1.Text = "KGR"
                txtKilos2.Text = "KGR"
                txtKilos4.Text = "KGR"
            End With
        End If

    End Sub

    Private Sub CargarPrecios(CodigoMercancia As String)

        Dim aTarifas() As String = Convert.ToString(ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM47")).Split(",")

        dgPrecios.ColumnCount = 3

        Dim aCamEqu() As String = {"Tarifa", "Precio", "nPrecio"}
        Dim aNomEqu() As String = {"Tarifa", "Precio", "Precio + IVA"}
        Dim aAncEqu() As Integer = {50, 65, 65}
        Dim aAliEqu() As Integer = {AlineacionDataGrid.Centro, AlineacionDataGrid.Derecha, AlineacionDataGrid.Derecha}
        Dim aForEqu() As String = {"", sFormatoNumero, sFormatoNumero}

        IniciarTabla(dgPrecios, dt, aCamEqu, aNomEqu, aAncEqu, aAliEqu, aForEqu, , , , , , False)

        For Each Str As String In aTarifas
            Dim nPrecio As Double = ft.DevuelveScalarDoble(myConn, " select precio_" + Str + " from jsmerctainv where codart = '" + CodigoMercancia + "' and id_emp = '" + jytsistema.WorkID + "' ")
            Dim nPrecioIVA As Double = nPrecio * (1 + ValorNumero(txtIVA.Text.Split("%")(0)) / 100)
            Dim aRow() As String = {Str, ft.FormatoNumero(nPrecio), ft.FormatoNumero(nPrecioIVA)}
            dgPrecios.Rows.Add(aRow)
        Next

    End Sub


    Private Sub DesactivarMarco0()

        ft.habilitarObjetos(False, True, txtCodigo, txtDescripcion, txtAlterno, txtBarras, txtCategoria, txtMarca, txtDivision, _
                         txtJerarquia, txtIVA, txtUbica1, txtUbica2, txtUbica3, cmbIVA, cmbUnidad, cmbCondicion, txtPesoUnidad, _
                         txtPresentacion)

        MenuBarra.Enabled = True

        ft.mensajeEtiqueta(lblInfo, "Haga click sobre cualquier bot�n de la barra menu...", Transportables.TipoMensaje.iAyuda)
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        If dt.Rows.Count = 0 Then
        Else
            Me.BindingContext(ds, nTabla).CancelCurrentEdit()
            AsignaTXT(nPosicion)
        End If

        DesactivarMarco0()
    End Sub
    Private Function Validado() As Boolean
        Validado = True
    End Function


    Private Sub btnBuscar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuscar.Click
        Dim f As New frmBuscar
        Dim Campos() As String = {"codart", "nomart", "alterno", "barras"}
        Dim Nombres() As String = {"C�digo", "Descripci�n", "Alterno", "C�digo Barras"}
        Dim Anchos() As Integer = {150, 650, 150, 150}
        f.Buscar(dt, Campos, Nombres, Anchos, Me.BindingContext(ds, nTabla).Position, "Mercanc�as...")
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


    Private Sub btnImprimir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnImprimir.Click
        '     Dim f As New jsMerRepParametros
        '     f.Cargar(TipoCargaFormulario.iShowDialog, ReporteMercancias.cTransferencia, "Transferencia/Nota de Consumo de mercanc�as", txtCodigo.Text)
        '     '        f = Nothing
    End Sub
    Private Sub cmbIVA_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbIVA.SelectedIndexChanged
        txtIVA.Text = ft.FormatoNumero(PorcentajeIVA(myConn, lblInfo, jytsistema.sFechadeTrabajo, cmbIVA.Text)) & "%"
    End Sub

    Private Sub txtCategoria_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtCategoria.TextChanged
        lblCategoria.Text = ft.DevuelveScalarCadena(myConn, " select descrip from jsconctatab where modulo = '" & FormatoTablaSimple(Modulo.iCategoriaMerca) & "' and codigo = '" & txtCategoria.Text & "' and id_emp ='" & jytsistema.WorkID & "' ")
    End Sub

    Private Sub txtMarca_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMarca.TextChanged
        lblMarca.Text = ft.DevuelveScalarCadena(myConn, " select descrip from jsconctatab where modulo = '" & FormatoTablaSimple(Modulo.iMarcaMerca) & "' and codigo = '" & txtMarca.Text & "' and id_emp ='" & jytsistema.WorkID & "' ")
    End Sub

    Private Sub txtDivision_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtDivision.TextChanged
        lblDivision.Text = ft.DevuelveScalarCadena(myConn, " select descrip from jsmercatdiv where division = '" & txtDivision.Text & "' and id_emp ='" & jytsistema.WorkID & "' ")
    End Sub

    Private Sub txtJerarquia_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtJerarquia.TextChanged
        lblJerarquia.Text = ft.DevuelveScalarCadena(myConn, " select descrip from jsmerencjer where  tipjer = '" & txtJerarquia.Text & "' and id_emp ='" & jytsistema.WorkID & "' ")
    End Sub


    Private Sub txtPromedioMeses_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPromedioMeses.TextChanged, _
    txtSugeridoDias.TextChanged
        ExistenciasPor(txtCodigo.Text, 0, ValorEntero(txtPromedioMeses.Text), ValorEntero(txtSugeridoDias.Text))

    End Sub
End Class
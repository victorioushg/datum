Imports MySql.Data.MySqlClient
Public Class jsPOSFacturasXConformar
    Private Const sModulo As String = "Facturas por confirmar"
    Private Const nTabla As String = "tblFCXConfirmar"

    Private myConn As New MySqlConnection
    Private ds As New DataSet
    Private dt As New DataTable
    Private ft As New Transportables

    Private strSQL As String = "select * from jsvenencpos where estatus = " & EstatusFactura.ePorConfirmar & " AND id_emp = '" & jytsistema.WorkID & "' "
    Private Posicion As Long

    Private n_Seleccionado As String
    Public Property Seleccionado() As String
        Get
            Return n_Seleccionado
        End Get
        Set(ByVal value As String)
            n_Seleccionado = value
        End Set
    End Property
    Public Sub Cargar(ByVal Mycon As MySqlConnection, ByVal TipoCarga As Integer)

        ' 0 = show() ; 1 = showdialog()

        myConn = Mycon

        ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
        dt = ds.Tables(nTabla)

        If ParametroPlus(myConn, Gestion.iPuntosdeVentas, "POSPARAM25") Then
            strSQL = "select * from jsvenencpos where estatus = " & EstatusFactura.ePorConfirmar & " AND id_emp = '" & jytsistema.WorkID & "' "
        Else
            strSQL = "select * from jsvenencpos where estatus = " & EstatusFactura.ePorConfirmar & " AND " _
                & " CODCAJ = '" & jytsistema.WorkBox & "' AND " _
                & " id_emp = '" & jytsistema.WorkID & "' "
        End If

        IniciarGrilla()
        AsignarTooltips()
        If dt.Rows.Count > 0 Then Posicion = 0
        AsignaTXT(Posicion, True)
        lblInfo.Text = " Haga click sobre cualquier bot�n de la barra menu ..."

        If TipoCarga = 0 Then
            Me.Show()
        Else
            Me.ShowDialog()
        End If


    End Sub
    Private Sub jsPOSFacturasXConfirmar_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        dt = Nothing
        ds = Nothing
    End Sub

    Private Sub jsPOSFacturasXConfirmar_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        InsertarAuditoria(myConn, MovAud.ientrar, sModulo, "")
    End Sub
    Private Sub AsignarTooltips()
        'Menu Barra 
        C1SuperTooltip1.SetToolTip(btnAgregar, "<B>Agregar</B> nuevo registro")
        C1SuperTooltip1.SetToolTip(btnEditar, "<B>Editar o mofificar</B> registro actual")
        C1SuperTooltip1.SetToolTip(btnEliminar, "<B>Eliminar</B> registro actual")
        C1SuperTooltip1.SetToolTip(btnBuscar, "<B>Buscar</B> registro deseado")
        C1SuperTooltip1.SetToolTip(btnSeleccionar, "<B>Seleccionar</B> registro actual")
        C1SuperTooltip1.SetToolTip(btnColumnas, "Coloca registros en modo <B>columnas</B>")
        C1SuperTooltip1.SetToolTip(btnPrimero, "ir al <B>primer</B> registro")
        C1SuperTooltip1.SetToolTip(btnSiguiente, "ir al <B>siguiente</B> registro")
        C1SuperTooltip1.SetToolTip(btnAnterior, "ir al registro <B>anterior</B>")
        C1SuperTooltip1.SetToolTip(btnUltimo, "ir al <B>�ltimo registro</B>")
        C1SuperTooltip1.SetToolTip(btnImprimir, "<B>Imprimir</B>")
        C1SuperTooltip1.SetToolTip(btnSalir, "<B>Cerrar</B> esta ventana")

    End Sub
    Private Sub AsignaTXT(ByVal nRow As Long, ByVal Actualiza As Boolean)

        Dim c As Integer = CInt(nRow)
        If Actualiza Then ds = DataSetRequery(ds, strSQL, myConn, nTabla, lblInfo)
        If c >= 0 AndAlso ds.Tables(nTabla).Rows.Count > 0 Then
            Me.BindingContext(ds, nTabla).Position = c
            With dt
                MostrarItemsEnMenuBarra(MenuBarra, nRow, .Rows.Count)
            End With
            dg.CurrentCell = dg(0, c)
        End If
        ft.ActivarMenuBarra(myConn, ds, dt, "", MenuBarra, jytsistema.sUsuario)

    End Sub
    Private Sub IniciarGrilla()
        Dim aCampos() As String = {"numfac", "emision", "nomcli", "tot_fac"}
        Dim aNombres() As String = {"N� Factura", "Emision", "Nombre Cliente", "Total Factura"}
        Dim aAnchos() As Integer = {70, 70, 350, 90}
        Dim aAlineacion() As Integer = {AlineacionDataGrid.Izquierda, AlineacionDataGrid.Centro, _
             AlineacionDataGrid.Izquierda, AlineacionDataGrid.Derecha}
        Dim aFormatos() As String = {"", sFormatoFecha, "", sFormatoNumero}
        IniciarTabla(dg, dt, aCampos, aNombres, aAnchos, aAlineacion, aFormatos)
    End Sub

    Private Sub btnAgregar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregar.Click
    End Sub

    Private Sub btnEditar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditar.Click
    End Sub

    Private Sub btnEliminar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEliminar.Click
    End Sub

    Private Sub btnBuscar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuscar.Click
    End Sub

    Private Sub btnSeleccionar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSeleccionar.Click
        Posicion = Me.BindingContext(ds, nTabla).Position
        If dt.Rows.Count > 0 Then
            Seleccionado = dt.Rows(Posicion).Item("numfac").ToString
            InsertarAuditoria(myConn, MovAud.iSeleccionar, Me.Text, "")
        End If
        Me.Close()
    End Sub

    Private Sub btnPrimero_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrimero.Click
        Me.BindingContext(ds, nTabla).Position = 0
        AsignaTXT(Me.BindingContext(ds, nTabla).Position, False)
    End Sub

    Private Sub btnAnterior_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAnterior.Click
        Me.BindingContext(ds, nTabla).Position -= 1
        AsignaTXT(Me.BindingContext(ds, nTabla).Position, False)
    End Sub

    Private Sub btnSiguiente_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSiguiente.Click
        Me.BindingContext(ds, nTabla).Position += 1
        AsignaTXT(Me.BindingContext(ds, nTabla).Position, False)
    End Sub

    Private Sub btnUltimo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUltimo.Click
        Me.BindingContext(ds, nTabla).Position = ds.Tables(nTabla).Rows.Count - 1
        AsignaTXT(Me.BindingContext(ds, nTabla).Position, False)
    End Sub

    Private Sub btnImprimir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub btnSalir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSalir.Click
        Me.Close()
    End Sub

    Private Sub dg_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dg.CellDoubleClick
        Posicion = Me.BindingContext(ds, nTabla).Position
        If dt.Rows.Count > 0 Then
            Seleccionado = dt.Rows(Posicion).Item("NUMFAC").ToString
            InsertarAuditoria(myConn, MovAud.iSeleccionar, Me.Text, "")
        End If
        Me.Close()
    End Sub
    Private Sub dg_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles _
        dg.RowHeaderMouseClick, dg.CellMouseClick
        Me.BindingContext(ds, nTabla).Position = e.RowIndex
        Posicion = Me.BindingContext(ds, nTabla).Position
        AsignaTXT(Posicion, False)
    End Sub

    Private Sub dg_RegionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dg.RegionChanged
        Posicion = Me.BindingContext(ds, nTabla).Position
    End Sub

End Class
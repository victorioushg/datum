Imports MySql.Data.MySqlClient

Public Class jsControlArcCausasCreditos
    Private Const sModulo As String = "Causas para cr�ditos"
    Private Const lRegion As String = "RibbonButton177"
    Private Const nTabla As String = "tblCausasCR"

    Private myConn As New MySqlConnection
    Private ds As New DataSet
    Private dt As New DataTable
    Private ft As New Transportables

    Private strSQL As String = ""
    Private CreditosDebitos As Integer
    Private Posicion As Long

    Private n_Seleccionado As String
    Private BindingSource1 As New BindingSource
    Private FindField As String

    Public Property Seleccionado() As String
        Get
            Return n_Seleccionado
        End Get
        Set(ByVal value As String)
            n_Seleccionado = value
        End Set
    End Property
    Public Sub Cargar(ByVal Mycon As MySqlConnection, ByVal Credito_Debito As Integer, ByVal TipoCarga As Integer)

        ' 0 = show() ; 1 = showdialog()

        myConn = Mycon
        CreditosDebitos = Credito_Debito
        Me.Text = IIf(Credito_Debito = 0, "Causas Creditos", "Causas D�bitos")
        strSQL = "select * from jsvencaudcr where credito_debito = " & Credito_Debito & " and id_emp = '" & jytsistema.WorkID & "' order by codigo"

        dt = ft.AbrirDataTable(ds, nTabla, myConn, strSQL)

        IniciarGrilla()
        AsignarTooltips()

        If dt.Rows.Count > 0 Then Posicion = 0

        AsignaTXT(Posicion, True)

        ft.mensajeEtiqueta(lblInfo, " Haga click sobre cualquier bot�n de la barra menu ...", Transportables.TipoMensaje.iAyuda)

        If TipoCarga = 0 Then
            Me.Show()
        Else
            Me.ShowDialog()
        End If


    End Sub
    Private Sub jsControlArcCausasCreditos_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        dt = Nothing
        ds = Nothing
    End Sub

    Private Sub jsControlArcCausasCreditos_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Private Sub AsignarTooltips()
        'Menu Barra 
        ft.colocaToolTip(C1SuperTooltip1, jytsistema.WorkLanguage, btnAgregar, btnEditar, btnEliminar, btnBuscar, btnSeleccionar, _
                          btnPrimero, btnSiguiente, btnAnterior, btnUltimo, btnImprimir, btnSalir)
    End Sub
    Private Sub AsignaTXT(ByVal nRow As Long, ByVal Actualiza As Boolean)

        If strSQL <> "" Then _
        dt = ft.MostrarFilaEnTabla(myConn, ds, nTabla, strSQL, Me.BindingContext, MenuBarra, dg, lRegion, _
                                    jytsistema.sUsuario, nRow, Actualiza)

    End Sub
    Private Sub IniciarGrilla()

        Dim aCampos() As String = {"codigo.Causa.50.C.", _
                                   "descrip.Descripci�n.380.I.", _
                                   "inventario.Movimiento Inventario.70.C.", _
                                   "validaunidad.Valida Unidad Venta.70.C.", _
                                   "ajustaprecio.Ajusta Precio.70.C.", _
                                   "estado.Mercanc�a Buen Estado.70.C."}

        ft.IniciarTablaPlus(dg, dt, aCampos)

        FindField = "codigo"
        BindingSource1.DataSource = dt
        BindingSource1.Filter = " codigo like '" & txtBuscar.Text & "%'"
        dg.DataSource = BindingSource1

    End Sub

    Private Sub btnAgregar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAgregar.Click
        Dim f As New jsControlArcCausasCreditosMovimientos
        f.Agregar(myConn, ds, dt, CreditosDebitos)
        If f.Apuntador >= 0 Then AsignaTXT(f.Apuntador, True)
        f = Nothing
    End Sub

    Private Sub btnEditar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEditar.Click
        Dim f As New jsControlArcCausasCreditosMovimientos
        f.Apuntador = Me.BindingContext(ds, nTabla).Position
        f.Editar(myConn, ds, dt, CreditosDebitos)
        AsignaTXT(f.Apuntador, True)
        f = Nothing
    End Sub

    Private Sub btnEliminar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEliminar.Click

        If NumeroDeRegistrosEnTabla(myConn, "jsvenrenncr", " causa = '" & dt.Rows(Posicion).Item("codigo") & "' ") > 0 And _
            CreditosDebitos = 0 Then
            ft.mensajeCritico("CAUSA NO ELIMINABLE...")
            Exit Sub
        End If

        If NumeroDeRegistrosEnTabla(myConn, "jsvenrenndb", " causa = '" & dt.Rows(Posicion).Item("codigo") & "' ") > 0 And _
            CreditosDebitos = 1 Then
            ft.mensajeCritico("CAUSA NO ELIMINABLE...")
            Exit Sub
        End If

        Posicion = Me.BindingContext(ds, nTabla).Position
        If ft.PreguntaEliminarRegistro() = Windows.Forms.DialogResult.Yes Then
            Dim aCampos() As String = {"codigo", "credito_debito", "id_emp"}
            Dim aString() As String = {dt.Rows(Posicion).Item("codigo"), CreditosDebitos, jytsistema.WorkID}
            Posicion = EliminarRegistros(myConn, lblInfo, ds, nTabla, "jsvencaudcr", strSQL, aCampos, aString, Posicion)
        End If
        AsignaTXT(Posicion, False)

    End Sub

    Private Sub btnBuscar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuscar.Click
        Dim f As New frmBuscar
        Dim Campos() As String = {"codigo", "descrip"}
        Dim Nombres() As String = {"Causa", "Descripci�n"}
        Dim Anchos() As Integer = {100, 450}
        f.Buscar(dt, Campos, Nombres, Anchos, Me.BindingContext(ds, nTabla).Position, "Causas para cr�ditos/devoluciones...")
        AsignaTXT(f.Apuntador, False)
        f = Nothing
    End Sub

    Private Sub btnSeleccionar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSeleccionar.Click
        Posicion = Me.BindingContext(ds, nTabla).Position
        If dt.Rows.Count > 0 Then
            Seleccionado = dt.Rows(Posicion).Item("codigo").ToString
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

    Private Sub btnSalir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSalir.Click
        Me.Close()
    End Sub

    Private Sub dg_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dg.CellDoubleClick
        Me.BindingContext(ds, nTabla).Position = e.RowIndex
        Posicion = e.RowIndex
        If dt.Rows.Count > 0 Then
            Seleccionado = dg.Rows(e.RowIndex).Cells(0).Value
            InsertarAuditoria(myConn, MovAud.iSeleccionar, Me.Text, "")
        End If
        Me.Close()
    End Sub
    Private Sub dg_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles _
        dg.RowHeaderMouseClick, dg.CellMouseClick
        Me.BindingContext(ds, nTabla).Position = e.RowIndex
        Posicion = Me.BindingContext(ds, nTabla).Position
        Seleccionado = dt.Rows(Posicion).Item("codigo").ToString
        AsignaTXT(Posicion, False)
    End Sub

    Private Sub dg_CellFormatting(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellFormattingEventArgs) Handles dg.CellFormatting
        Select Case e.ColumnIndex
            Case 2, 3, 4, 5
                e.Value = IIf(e.Value = 0, "No", "Si")
        End Select
    End Sub
    Private Sub dg_ColumnHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles dg.ColumnHeaderMouseClick
        FindField = dt.Columns(e.ColumnIndex).ColumnName
        lblBuscar.Text = dg.Columns(e.ColumnIndex).HeaderText
    End Sub

    Private Sub txtBuscar_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtBuscar.TextChanged
        BindingSource1.DataSource = dt
        If dt.Columns(FindField).DataType Is GetType(String) Then _
            BindingSource1.Filter = FindField & " like '%" & txtBuscar.Text & "%'"
        dg.DataSource = BindingSource1
    End Sub

  
End Class
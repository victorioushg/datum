Imports MySql.Data.MySqlClient
Public Class jsBanArcReposicionCajaPlus

    Private Const sModulo As String = "Reposici�n de saldo en caja"

    Private MyConn As New MySqlConnection
    Private ds As DataSet
    Private dt As DataTable
    Private dtRepos As New DataTable
    Private ft As New Transportables

    Private CodigoCaja As String
    Private CodigoBanco As String
    Private n_Apuntador As Long
    Private Renglon As String

    Private isel As Integer
    Private dSel As Double

    Private strSQL As String = ""
    Private nTablaRepos As String = "tblrepos"
    Private m_SortingColumn As ColumnHeader

    Private aTipo() As String = {"Reposici�n de Caja", "Adelanto a Caja Chica"}
    Private aFormaPago() As String = {"Cheque", "Transferencia"}
    Private aSeleccion() As String = {"Ninguno", "Todos", "Por fecha"}

    Public Property Apuntador() As Long
        Get
            Return n_Apuntador
        End Get
        Set(ByVal value As Long)
            n_Apuntador = value
        End Set
    End Property
    Public Sub Cargar(ByVal MyCon As MySqlConnection, ByVal dsMov As DataSet, ByVal dtMov As DataTable, _
                      ByVal CodCaja As String, ByVal CodBanco As String)
        MyConn = MyCon
        ds = dsMov
        dt = dtMov
        CodigoCaja = Mid(CodCaja, 1, 2)
        CodigoBanco = CodBanco
        Me.Text = Me.Text & " " & CodCaja
        If dt.Rows.Count = 0 Then Apuntador = -1
        IniciarTXT()
        Me.ShowDialog()

    End Sub
    Private Sub IniciarTXT()

        ft.habilitarObjetos(False, True, txtCuadre, txtFecha, txtFechaSeleccion, txtImporte, txtSaldo, txtiSel, txtComprobante)

        ft.RellenaCombo(aTipo, cmbTipo)
        ft.RellenaCombo(aFormaPago, cmbFormaPago)
        ft.RellenaCombo(aSeleccion, cmbSeleccion)

        ft.visualizarObjetos(False, txtFechaSeleccion, btnFechaSeleccion)

        txtFecha.Text = ft.FormatoFecha(jytsistema.sFechadeTrabajo)
        txtFechaSeleccion.Text = ft.FormatoFecha(jytsistema.sFechadeTrabajo)
        txtDocumento.Text = ""
        txtComprobante.Text = Contador(MyConn, lblInfo, Gestion.iBancos, "BANNUMRCC", "03")
        txtBeneficiario.Text = ""
        txtSaldo.Text = ft.FormatoNumero(CalculaSaldoCajaPorFP(MyConn, CodigoCaja, "", lblInfo))
        txtImporte.Text = ft.FormatoNumero(0.0)
        txtAdicional.Text = ft.FormatoNumero(0.0)
        txtiSel.Text = ft.FormatoEntero(0)

        strSQL = "select a.fecha, a.nummov, a.tipomov, a.formpag, a.numpag, a.refpag, a.importe, a.concepto, a.origen, a.cantidad, a.prov_cli, b.nombre, a.id_emp " _
                    & " from jsbantracaj a " _
                    & " left join jsprocatpro b on (a.prov_cli = b.codpro and a.id_emp = b.id_emp)" _
                    & " where " _
                    & " a.id_emp = '" & jytsistema.WorkID & "' and " _
                    & " a.deposito = '' and " _
                    & " a.formpag IN ('EF') and " _
                    & " a.fecha <= '" & ft.FormatoFechaMySQL(jytsistema.sFechadeTrabajo) & "' and " _
                    & " a.caja = '" & CodigoCaja & "' and " _
                    & " a.ejercicio = '" & jytsistema.WorkExercise & "' " _
                    & " order by a.fecha desc, a.formpag "


        ds = DataSetRequery(ds, strSQL, MyConn, nTablaRepos, lblInfo)
        dtRepos = ds.Tables(nTablaRepos)

        CargaListViewDesdeCAJAT(lv, dtRepos)

    End Sub

    Private Sub jsBanReposicionCaja_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ft = Nothing
    End Sub

    Private Sub jsBanReposicionCaja_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.Tag = sModulo
        InsertarAuditoria(MyConn, MovAud.ientrar, sModulo, CodigoCaja)
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        InsertarAuditoria(MyConn, MovAud.iSalir, sModulo, CodigoCaja)
        Me.Close()
    End Sub
    Private Function Validado() As Boolean
        Validado = False
        If Trim(txtDocumento.Text) = "" Then
            ft.mensajeAdvertencia("Debe indicar un n�mero de documento v�lido ...")
            ft.enfocarTexto(txtDocumento)
            Return False
        Else
            Dim aCampos() As String = {"codban", "tipomov", "numdoc", "id_emp"}
            Dim aValores() As String = {CodigoBanco, "CH", txtDocumento.Text, jytsistema.WorkID}
            If qFound(MyConn, lblInfo, "jsbantraban", aCampos, aValores) Then
                ft.mensajeAdvertencia("Est� documento ya fu� incluido, verifique ...")
                ft.enfocarTexto(txtDocumento)
                Return False
            End If
        End If

        If Not ft.isNumeric(txtImporte.Text) Then
            ft.mensajeAdvertencia("El importe  debe ser num�rico ...")
            Return False
        End If

        If Not ft.isNumeric(txtAdicional.Text) Then
            ft.mensajeAdvertencia("El monto adicional debe ser num�rico ...")
            ft.enfocarTexto(txtAdicional)
        End If

        If Math.Abs(ValorNumero(txtImporte.Text)) + ValorNumero(txtAdicional.Text) <= 0 Then
            ft.mensajeAdvertencia("El importe de reposici�n debe ser mayor a cero...")
            Return False
        End If

        If Trim(txtBeneficiario.Text) = "" Then
            ft.mensajeAdvertencia("Debe indicar un nombre de beneficiario...")
            ft.enfocarTexto(txtBeneficiario)
            Return False
        End If

        Validado = True
    End Function


    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        If Validado() Then

            Apuntador = dt.Rows.Count
            Renglon = Contador(MyConn, lblInfo, Gestion.iBancos, "BANNUMTRACAJ", "05")

            InsertEditBANCOSMovimientoBanco(MyConn, lblInfo, True, CDate(txtFecha.Text), txtDocumento.Text, _
                "CH", CodigoBanco, CodigoCaja, "REPOSICION DE SALDO EN CAJA " & CodigoCaja, -1 * Math.Abs(ValorNumero(txtCuadre.Text)), _
                "BAN", txtDocumento.Text, txtBeneficiario.Text, txtComprobante.Text, "0", jytsistema.sFechadeTrabajo, jytsistema.sFechadeTrabajo, _
                "CH", "", jytsistema.sFechadeTrabajo, "0", "", "")

            IncluirImpuestoDebitoBancario(MyConn, lblInfo, "CH", CodigoBanco, txtDocumento.Text, CDate(txtFecha.Text), _
                                          ValorNumero(txtCuadre.Text))

            InsertEditBANCOSRenglonCaja(MyConn, lblInfo, True, CodigoCaja, Renglon, CDate(txtFecha.Text), "BAN", _
                 "EN", txtDocumento.Text, "EF", txtDocumento.Text, CodigoBanco, Math.Abs(ValorNumero(txtImporte.Text)), "", "REPOSICION DE SALDO EN CAJA", txtComprobante.Text, jytsistema.MyDate, 1, _
                  "", "", "", jytsistema.MyDate, "", "", "1")

            If ValorNumero(txtAdicional.Text) > 0 Then _
                InsertEditBANCOSRenglonCaja(MyConn, lblInfo, True, CodigoCaja, Renglon, CDate(txtFecha.Text), "BAN", _
                     "EN", txtDocumento.Text, "EF", txtDocumento.Text, CodigoBanco, Math.Abs(ValorNumero(txtAdicional.Text)), "", "AUMENTO DE SALDO EN CAJA", "", jytsistema.MyDate, 1, _
                      "", "", "", jytsistema.MyDate, "", "", "1")

            Dim lvCont As Integer

            For lvCont = 0 To lv.Items.Count - 1
                If lv.Items(lvCont).Checked Then
                    ft.Ejecutar_strSQL(myconn, "UPDATE jsbantracaj SET DEPOSITO = '" & txtComprobante.Text & "',  " _
                        & " fecha_dep = '" & ft.FormatoFechaMySQL(CDate(txtFecha.Text)) & "', " _
                        & " codban = '' " _
                        & " where " _
                        & " caja = '" & Mid(CodigoCaja, 1, 2) & "' and " _
                        & " tipomov ='" & lv.Items(lvCont).SubItems(1).Text & "' and " _
                        & " fecha = '" & ft.FormatoFechaMySQL(CDate(lv.Items(lvCont).Text)) & "' and " _
                        & " nummov = '" & lv.Items(lvCont).SubItems(2).Text & "' and " _
                        & " formpag = 'EF' and " _
                        & " numpag = '" & dtRepos.Rows(lvCont).Item("NUMPAG").ToString & "' and " _
                        & " refpag = '" & dtRepos.Rows(lvCont).Item("REFPAG").ToString & "' and " _
                        & " importe = " & ValorNumero(dtRepos.Rows(lvCont).Item("IMPORTE").ToString) & "  and " _
                        & " origen = '" & dtRepos.Rows(lvCont).Item("ORIGEN").ToString & "' and " _
                        & " id_emp ='" & jytsistema.WorkID & "' and " _
                        & " ejercicio = '" & jytsistema.WorkExercise & "'")
                End If
            Next

            'Imprimir Comprobante 
            Dim resp As Integer
            Dim fr As New jsBanRepParametros
            resp = MsgBox(" �Desea imprimir comprobante de pago? ", MsgBoxStyle.YesNo, sModulo)
            If resp = MsgBoxResult.Yes Then
                fr.Cargar(TipoCargaFormulario.iShowDialog, ReporteBancos.cComprobanteDeEgreso, "COMPROBANTE DE PAGO", CodigoBanco, txtComprobante.Text)
            End If
            'Imprimir Cheque 
            resp = MsgBox(" �Desea imprimir CHEQUE? ", MsgBoxStyle.YesNo, sModulo)
            If resp = MsgBoxResult.Yes Then
                fr.Cargar(TipoCargaFormulario.iShowDialog, ReporteBancos.cCheque, "CHEQUE", CodigoBanco, txtComprobante.Text)
            End If
            'Imprimir Relaci�n

            resp = MsgBox(" �Desea imprimir RELACION DE REPOSICION DE SALDO EN CAJA? ", MsgBoxStyle.YesNo, sModulo)
            If resp = MsgBoxResult.Yes Then
                fr.Cargar(TipoCargaFormulario.iShowDialog, ReporteBancos.cReposicionSaldoCaja, "REPOSICION DE SALDO EN CAJA", , txtComprobante.Text)
            End If

            fr = Nothing
            InsertarAuditoria(MyConn, MovAud.iSalir, sModulo, CodigoCaja & " " & txtDocumento.Text)
            Me.Close()

        End If

    End Sub

    Private Sub txtDocumento_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDocumento.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Indique el n�mero de documento para este movimiento ...", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub txtImporte_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtImporte.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Indique el monto � importe para este movimiento ...", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub txtBeneficiario_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtBeneficiario.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Indique NOMBRE del beneficiario de este cheque ", Transportables.TipoMensaje.iInfo)
    End Sub


    Private Sub btnFecha_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFecha.Click
        txtFecha.Text = SeleccionaFecha(CDate(txtFecha.Text), Me, btnFecha)
    End Sub

    Private Sub txtAdicional_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtAdicional.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Indique monto adicional con el cual quiere aumentar la caja chica ", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub txtAdicional_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtAdicional.KeyPress
        e.Handled = ft.validaNumeroEnTextbox(e)
    End Sub

    Private Sub txtSaldo_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSaldo.TextChanged, _
        txtImporte.TextChanged, txtAdicional.TextChanged
        txtCuadre.Text = ft.FormatoNumero(Math.Abs(ValorNumero(txtImporte.Text)) + ValorNumero(txtAdicional.Text))
    End Sub
    Private Sub lv_ItemChecked(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckedEventArgs) Handles lv.ItemChecked

        If e.Item.SubItems(1).Text = "SA" Then

            If e.Item.Checked Then
                isel += 1
                dSel += CDbl(e.Item.SubItems(4).Text)
            Else
                isel -= 1
                dSel -= CDbl(e.Item.SubItems(4).Text)
            End If

            If isel < 0 Then
                isel = 0
                dSel = 0.0
            End If

            txtiSel.Text = ft.FormatoEntero(isel)
            txtImporte.Text = ft.FormatoNumero(dSel)
        Else
            e.Item.Checked = False
        End If

    End Sub
    Private Sub lv_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lv.ColumnClick
        Dim new_sorting_column As ColumnHeader = lv.Columns(e.Column)
        Dim sort_order As System.Windows.Forms.SortOrder
        If m_SortingColumn Is Nothing Then
            sort_order = SortOrder.Ascending
        Else
            If new_sorting_column.Equals(m_SortingColumn) Then
                If m_SortingColumn.Text.StartsWith("> ") Then
                    sort_order = SortOrder.Descending
                Else
                    sort_order = SortOrder.Ascending
                End If
            Else
                sort_order = SortOrder.Ascending
            End If
            m_SortingColumn.Text = m_SortingColumn.Text.Substring(2)
        End If
        m_SortingColumn = new_sorting_column
        If sort_order = SortOrder.Ascending Then
            m_SortingColumn.Text = "> " & m_SortingColumn.Text
        Else
            m_SortingColumn.Text = "< " & m_SortingColumn.Text
        End If
        lv.ListViewItemSorter = New clsListviewSorter(e.Column, sort_order)
        lv.Sort()
    End Sub

    Private Sub lv_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lv.SelectedIndexChanged

    End Sub

    Private Sub btnFechaSeleccion_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFechaSeleccion.Click
        txtFechaSeleccion.Text = ft.FormatoFecha(SeleccionaFecha(CDate(txtFechaSeleccion.Text), Me, btnFechaSeleccion))
    End Sub

    Private Sub btnGo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGo.Click
        Dim kCont As Integer
        Select Case cmbSeleccion.SelectedIndex
            Case 0 'Ninguno 
                For kCont = 0 To lv.Items.Count - 1
                    If lv.Items(kCont).SubItems(1).Text = "SA" Then lv.Items(kCont).Checked = False
                Next
            Case 1 'Todos
                For kCont = 0 To lv.Items.Count - 1
                    If lv.Items(kCont).SubItems(1).Text = "SA" Then lv.Items(kCont).Checked = True
                Next
            Case 2 ' Por fecha
                For kCont = 0 To lv.Items.Count - 1
                    If ft.FormatoFechaMySQL(CDate(lv.Items(kCont).SubItems(0).Text)) = ft.FormatoFechaMySQL(CDate(txtFechaSeleccion.Text)) Then lv.Items(kCont).Checked = True
                Next
        End Select
    End Sub

    Private Sub cmbSeleccion_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbSeleccion.SelectedIndexChanged
        Select Case cmbSeleccion.SelectedIndex
            Case 2
                ft.visualizarObjetos(True, txtFechaSeleccion, btnFechaSeleccion)
            Case Else
                ft.visualizarObjetos(False, txtFechaSeleccion, btnFechaSeleccion)
        End Select
    End Sub

    Private Sub cmbTipo_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbTipo.SelectedIndexChanged
        ft.visualizarObjetos(Not Convert.ToBoolean(cmbTipo.SelectedIndex), cmbSeleccion, btnGo, txtFechaSeleccion, btnFechaSeleccion)
        Select Case cmbTipo.SelectedIndex
            Case 0
            Case Else

        End Select
    End Sub

End Class
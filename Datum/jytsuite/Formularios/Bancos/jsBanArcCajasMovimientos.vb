Imports MySql.Data.MySqlClient
Public Class jsBanArcCajasMovimientos
    Private Const sModulo As String = "Movimiento de caja"
    Private Const nTabla As String = "tarjetas"
    Private strSQL As String = "select * from jsconctatar where id_emp = '" & jytsistema.WorkID & "' order by codtar"

    Private MyConn As New MySqlConnection
    Private ds As DataSet
    Private dt As DataTable
    Private dtTar As DataTable
    Private ft As New Transportables

    Private i_modo As Integer
    Private aTipo() As String = {"Entrada", "Salida"}
    Private aTipoR() As String = {"EN", "SA"}
    Private aFormaPag() As String = {"Efectivo", "Cheque", "Tarjeta", "Cheque Alimentaci�n", "Otra"}
    Private aFormaPagR() As String = {"EF", "CH", "TA", "CT", "OT"}
    Private CodigoCaja As String
    Private n_Apuntador As Long
    Private Renglon As String
    Public Property Apuntador() As Long
        Get
            Return n_Apuntador
        End Get
        Set(ByVal value As Long)
            n_Apuntador = value
        End Set
    End Property
    Public Sub Agregar(ByVal MyCon As MySqlConnection, ByVal dsMov As DataSet, ByVal dtMov As DataTable, ByVal CodCaja As String)
        i_modo = movimiento.iAgregar
        MyConn = MyCon
        ds = dsMov
        dt = dtMov
        CodigoCaja = CodCaja
        If dt.Rows.Count = 0 Then Apuntador = -1
        IniciarTXT()
        Me.ShowDialog()
    End Sub
    Private Sub IniciarTXT()
        txtFecha.Text = ft.FormatoFecha(jytsistema.sFechadeTrabajo)
        ft.RellenaCombo(aTipo, cmbTipo, 0)
        ft.RellenaCombo(aFormaPag, cmbFormaPago, 0)
        txtDocumento.Text = ""
        txtDocPago.Text = ""
        txtRefPago.Text = ""
        txtImporte.Text = ft.FormatoNumero(0.0)
    End Sub

    Public Sub Editar(ByVal MyCon As MySqlConnection, ByVal dsMov As DataSet, ByVal dtMov As DataTable, ByVal codCaja As String)
        i_modo = movimiento.iEditar
        MyConn = MyCon
        ds = dsMov
        dt = dtMov
        CodigoCaja = codCaja
        AsignarTXT(Apuntador)
        Me.ShowDialog()
    End Sub
    Private Sub AsignarTXT(ByVal nPosicion As Integer)
        With dt
            txtFecha.Text = ft.FormatoFecha(CDate(.Rows(nPosicion).Item("fecha").ToString))
            ft.RellenaCombo(aTipo, cmbTipo, Array.IndexOf(aTipoR, .Rows(nPosicion).Item("tipomov")))
            ft.RellenaCombo(aFormaPag, cmbFormaPago, Array.IndexOf(aFormaPagR, .Rows(nPosicion).Item("formpag")))
            txtDocumento.Text = .Rows(nPosicion).Item("nummov")
            txtDocPago.Text = IIf(IsDBNull(.Rows(nPosicion).Item("numpag")), "", .Rows(nPosicion).Item("numpag"))
            txtRefPago.Text = IIf(IsDBNull(.Rows(nPosicion).Item("refpag")), "", .Rows(nPosicion).Item("refpag"))
            txtImporte.Text = ft.FormatoNumero(Math.Abs(.Rows(nPosicion).Item("importe")))
            Renglon = .Rows(nPosicion).Item("renglon")
        End With
    End Sub

    Private Sub jsBanCajasMovimientos_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        dtTar = Nothing
        ft = Nothing
    End Sub

    Private Sub jsBanCajasMovimientos_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.Tag = sModulo
        ft.habilitarObjetos(False, True, txtFecha)
        InsertarAuditoria(MyConn, MovAud.ientrar, sModulo, CodigoCaja)
        ds = DataSetRequery(ds, strSQL, MyConn, nTabla, lblInfo)
        dtTar = ds.Tables(nTabla)
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        InsertarAuditoria(MyConn, MovAud.iSalir, sModulo, CodigoCaja)
        Me.Close()
    End Sub
    Private Function Validado() As Boolean


        If Trim(txtDocumento.Text) = "" Then
            ft.mensajeAdvertencia("Debe indicar un n�mero de documento v�lido ...")
            ft.enfocarTexto(txtDocumento)
            Return False
        Else
            Dim aCampos() As String = {"tipomov", "nummov", "id_emp"}
            Dim aValores() As String = {CStr(aTipoR(cmbTipo.SelectedIndex)), txtDocumento.Text, jytsistema.WorkID}

            If qFound(MyConn, lblInfo, "jsbantracaj", aCampos, aValores) AndAlso i_modo = movimiento.iAgregar Then
                ft.mensajeAdvertencia("Est� documento ya fu� incluido, verifique ...")
                ft.enfocarTexto(txtDocumento)
                Return False
            End If
        End If

        If cmbFormaPago.SelectedIndex >= 1 AndAlso txtDocPago.Text = "" Then
            ft.mensajeAdvertencia("Debe indicar un n�mero de pago v�lido ...")
            ft.enfocarTexto(txtDocPago)
            Return False
        End If

        If cmbFormaPago.SelectedIndex >= 1 AndAlso txtRefPago.Text = "" Then
            ft.mensajeAdvertencia("Debe indicar una referencia de pago v�lida ...")
            ft.enfocarTexto(txtRefPago)
            Return False
        End If

        If cmbTipo.SelectedIndex = 1 AndAlso cmbFormaPago.SelectedIndex >= 1 Then
            ft.mensajeAdvertencia("Una salida de caja s�lo puede ser en efectivo ...")
            cmbFormaPago.Focus()
            Return False
        End If

        If Not ft.isNumeric(txtImporte.Text) Then
            ft.mensajeAdvertencia("El importe  debe ser num�rico ...")
            ft.enfocarTexto(txtImporte)
            Return False
        End If

        Dim aAdicionales() As String = {" caja = '" & CodigoCaja & "' AND "}
        If FechaUltimoBloqueo(MyConn, "jsbantracaj", aAdicionales) >= Convert.ToDateTime(txtFecha.Text) Then
            ft.mensajeEtiqueta(lblInfo, "FECHA MENOR QUE ULTIMA FECHA DE CIERRE...", Transportables.tipoMensaje.iAdvertencia)
            Return False
        End If

        Return True

    End Function


    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        If Validado() Then
            Dim Insertar As Boolean = False
            If i_modo = movimiento.iAgregar Then
                Insertar = True
                Apuntador = dt.Rows.Count
            End If
            InsertEditBANCOSRenglonCaja(MyConn, lblInfo, Insertar, CodigoCaja, Renglon, CDate(txtFecha.Text), "CAJ", _
                 aTipoR(cmbTipo.SelectedIndex), txtDocumento.Text, aFormaPagR(cmbFormaPago.SelectedIndex), _
                 txtDocPago.Text, txtRefPago.Text, IIf(cmbTipo.SelectedIndex = 0, 1, -1) * ValorNumero(txtImporte.Text), "", "", "", jytsistema.MyDate, 1, _
                 "", "", "", MyDate, "", "", "0")

            InsertarAuditoria(MyConn, MovAud.iSalir, sModulo, CodigoCaja & " " & txtDocumento.Text)
            Me.Close()
        End If
    End Sub

    Private Sub txtDocumento_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDocumento.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Indique el n�mero de documento para este movimiento ...", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub txtDocPago_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDocPago.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Indique el n�mero de documento de pago para este movimiento ...", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub txtRefPago_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtRefPago.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Indique la referencia de pago ...", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub txtImporte_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtImporte.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Indique el monto � importe para este movimiento ...", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub cmbTipo_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbTipo.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Seleccione el tipo de movimiento ...", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub cmbFormaPago_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbFormaPago.GotFocus
        ft.mensajeEtiqueta(lblInfo, "Seleccione la forma de pago para este movimiento ...", Transportables.TipoMensaje.iInfo)
    End Sub

    Private Sub cmbFormaPago_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbFormaPago.Click, _
        cmbTipo.Click, cmbFormaPago.SelectedIndexChanged, cmbTipo.SelectedIndexChanged
        If cmbTipo.SelectedIndex = 0 AndAlso cmbFormaPago.SelectedIndex = 2 Then
            ft.VisualizarObjetos(True, btnTarjeta)
            ft.habilitarObjetos(False, True, txtRefPago)

        Else
            txtRefPago.Enabled = True
            btnTarjeta.Visible = False
            ft.habilitarObjetos(True, True, txtRefPago)
            ft.VisualizarObjetos(False, btnTarjeta)
        End If
    End Sub

    Private Sub btnTarjeta_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTarjeta.Click
        Dim f As New jsControlArcTarjetas
        f.Cargar(MyConn, TipoCargaFormulario.iShowDialog)
        If dtTar.Rows.Count > 0 Then
            txtRefPago.Text = f.Seleccionado
        Else
            txtRefPago.Text = ""
        End If
        f = Nothing
    End Sub

    Private Sub btnFecha_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFecha.Click
        txtFecha.Text = SeleccionaFecha(CDate(txtFecha.Text), Me, btnFecha)

    End Sub

    Private Sub txtImporte_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtImporte.KeyPress
        e.Handled = ft.validaNumeroEnTextbox(e)
    End Sub
End Class
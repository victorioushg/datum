<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class jsVenProCierreDiario
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(jsVenProCierreDiario))
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.grpCaja = New System.Windows.Forms.GroupBox()
        Me.lblLeyenda = New System.Windows.Forms.Label()
        Me.grpTotales = New System.Windows.Forms.GroupBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblProgreso = New System.Windows.Forms.Label()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.grpAceptarSalir = New System.Windows.Forms.TableLayoutPanel()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.C1PictureBox1 = New C1.Win.C1Input.C1PictureBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtFechaProceso = New System.Windows.Forms.TextBox()
        Me.btnFecha = New System.Windows.Forms.Button()
        Me.chk1 = New System.Windows.Forms.CheckBox()
        Me.chk2 = New System.Windows.Forms.CheckBox()
        Me.chk3 = New System.Windows.Forms.CheckBox()
        Me.chk4 = New System.Windows.Forms.CheckBox()
        Me.chk5 = New System.Windows.Forms.CheckBox()
        Me.chk6 = New System.Windows.Forms.CheckBox()
        Me.chk7 = New System.Windows.Forms.CheckBox()
        Me.chkCierre = New System.Windows.Forms.CheckBox()
        Me.chkSeleccionar = New System.Windows.Forms.CheckBox()
        Me.grpCaja.SuspendLayout()
        Me.grpTotales.SuspendLayout()
        Me.grpAceptarSalir.SuspendLayout()
        CType(Me.C1PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblInfo
        '
        Me.lblInfo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblInfo.BackColor = System.Drawing.Color.FromArgb(CType(CType(252, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(217, Byte), Integer))
        Me.lblInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.lblInfo.Location = New System.Drawing.Point(-1, 460)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(566, 27)
        Me.lblInfo.TabIndex = 80
        '
        'grpCaja
        '
        Me.grpCaja.BackColor = System.Drawing.Color.Transparent
        Me.grpCaja.Controls.Add(Me.lblLeyenda)
        Me.grpCaja.Location = New System.Drawing.Point(1, 58)
        Me.grpCaja.Name = "grpCaja"
        Me.grpCaja.Size = New System.Drawing.Size(730, 189)
        Me.grpCaja.TabIndex = 82
        Me.grpCaja.TabStop = False
        '
        'lblLeyenda
        '
        Me.lblLeyenda.BackColor = System.Drawing.Color.White
        Me.lblLeyenda.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.lblLeyenda.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLeyenda.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(71, Byte), Integer), CType(CType(182, Byte), Integer))
        Me.lblLeyenda.Location = New System.Drawing.Point(12, 15)
        Me.lblLeyenda.Name = "lblLeyenda"
        Me.lblLeyenda.Size = New System.Drawing.Size(707, 159)
        Me.lblLeyenda.TabIndex = 90
        Me.lblLeyenda.Text = " "
        '
        'grpTotales
        '
        Me.grpTotales.BackColor = System.Drawing.Color.Transparent
        Me.grpTotales.Controls.Add(Me.Label3)
        Me.grpTotales.Controls.Add(Me.lblProgreso)
        Me.grpTotales.Controls.Add(Me.ProgressBar1)
        Me.grpTotales.Enabled = False
        Me.grpTotales.Location = New System.Drawing.Point(3, 369)
        Me.grpTotales.Name = "grpTotales"
        Me.grpTotales.Size = New System.Drawing.Size(728, 85)
        Me.grpTotales.TabIndex = 83
        Me.grpTotales.TabStop = False
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(11, 16)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(87, 20)
        Me.Label3.TabIndex = 18
        Me.Label3.Text = "Progreso ..."
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblProgreso
        '
        Me.lblProgreso.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblProgreso.Location = New System.Drawing.Point(89, 16)
        Me.lblProgreso.Name = "lblProgreso"
        Me.lblProgreso.Size = New System.Drawing.Size(628, 37)
        Me.lblProgreso.TabIndex = 17
        Me.lblProgreso.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ProgressBar1.Location = New System.Drawing.Point(6, 56)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(711, 20)
        Me.ProgressBar1.TabIndex = 19
        '
        'grpAceptarSalir
        '
        Me.grpAceptarSalir.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpAceptarSalir.ColumnCount = 2
        Me.grpAceptarSalir.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.grpAceptarSalir.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.grpAceptarSalir.Controls.Add(Me.btnCancel, 1, 0)
        Me.grpAceptarSalir.Controls.Add(Me.btnOK, 0, 0)
        Me.grpAceptarSalir.Location = New System.Drawing.Point(569, 460)
        Me.grpAceptarSalir.Name = "grpAceptarSalir"
        Me.grpAceptarSalir.RowCount = 1
        Me.grpAceptarSalir.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.grpAceptarSalir.Size = New System.Drawing.Size(165, 30)
        Me.grpAceptarSalir.TabIndex = 85
        '
        'btnCancel
        '
        Me.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Image = Global.Datum.My.Resources.Resources.button_cancel
        Me.btnCancel.Location = New System.Drawing.Point(85, 3)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(76, 24)
        Me.btnCancel.TabIndex = 1
        Me.btnCancel.Text = "Cancelar"
        Me.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        '
        'btnOK
        '
        Me.btnOK.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.btnOK.Image = Global.Datum.My.Resources.Resources.button_ok
        Me.btnOK.Location = New System.Drawing.Point(3, 3)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(76, 24)
        Me.btnOK.TabIndex = 0
        Me.btnOK.Text = "Aceptar"
        Me.btnOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        '
        'C1PictureBox1
        '
        Me.C1PictureBox1.Image = Global.Datum.My.Resources.Resources.banda_amarilla
        Me.C1PictureBox1.Location = New System.Drawing.Point(92, 1)
        Me.C1PictureBox1.Name = "C1PictureBox1"
        Me.C1PictureBox1.Size = New System.Drawing.Size(639, 61)
        Me.C1PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.C1PictureBox1.TabIndex = 86
        Me.C1PictureBox1.TabStop = False
        '
        'Label9
        '
        Me.Label9.BackColor = System.Drawing.Color.FromArgb(CType(CType(252, Byte), Integer), CType(CType(216, Byte), Integer), CType(CType(22, Byte), Integer))
        Me.Label9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(71, Byte), Integer), CType(CType(182, Byte), Integer))
        Me.Label9.Location = New System.Drawing.Point(1, 41)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(245, 21)
        Me.Label9.TabIndex = 87
        Me.Label9.Text = "Ventas : Cierre Diario"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label10
        '
        Me.Label10.BackColor = System.Drawing.Color.FromArgb(CType(CType(252, Byte), Integer), CType(CType(216, Byte), Integer), CType(CType(22, Byte), Integer))
        Me.Label10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Label10.Font = New System.Drawing.Font("Consolas", 21.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(71, Byte), Integer), CType(CType(182, Byte), Integer))
        Me.Label10.Location = New System.Drawing.Point(1, 1)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(245, 40)
        Me.Label10.TabIndex = 88
        Me.Label10.Text = "Datum"
        Me.Label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(14, 259)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(96, 20)
        Me.Label1.TabIndex = 89
        Me.Label1.Text = "Fecha : "
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'txtFechaProceso
        '
        Me.txtFechaProceso.Location = New System.Drawing.Point(70, 260)
        Me.txtFechaProceso.MaxLength = 25
        Me.txtFechaProceso.Name = "txtFechaProceso"
        Me.txtFechaProceso.Size = New System.Drawing.Size(86, 20)
        Me.txtFechaProceso.TabIndex = 90
        Me.txtFechaProceso.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'btnFecha
        '
        Me.btnFecha.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnFecha.Location = New System.Drawing.Point(162, 260)
        Me.btnFecha.Name = "btnFecha"
        Me.btnFecha.Size = New System.Drawing.Size(25, 20)
        Me.btnFecha.TabIndex = 132
        Me.btnFecha.Text = "���"
        Me.btnFecha.UseVisualStyleBackColor = True
        '
        'chk1
        '
        Me.chk1.AutoSize = True
        Me.chk1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chk1.Location = New System.Drawing.Point(240, 263)
        Me.chk1.Name = "chk1"
        Me.chk1.Size = New System.Drawing.Size(93, 17)
        Me.chk1.TabIndex = 133
        Me.chk1.Text = "Mercanc�as"
        Me.chk1.UseVisualStyleBackColor = True
        '
        'chk2
        '
        Me.chk2.AutoSize = True
        Me.chk2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chk2.Location = New System.Drawing.Point(240, 286)
        Me.chk2.Name = "chk2"
        Me.chk2.Size = New System.Drawing.Size(88, 17)
        Me.chk2.TabIndex = 134
        Me.chk2.Text = "Categor�as"
        Me.chk2.UseVisualStyleBackColor = True
        '
        'chk3
        '
        Me.chk3.AutoSize = True
        Me.chk3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chk3.Location = New System.Drawing.Point(240, 309)
        Me.chk3.Name = "chk3"
        Me.chk3.Size = New System.Drawing.Size(67, 17)
        Me.chk3.TabIndex = 135
        Me.chk3.Text = "Marcas"
        Me.chk3.UseVisualStyleBackColor = True
        '
        'chk4
        '
        Me.chk4.AutoSize = True
        Me.chk4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chk4.Location = New System.Drawing.Point(240, 332)
        Me.chk4.Name = "chk4"
        Me.chk4.Size = New System.Drawing.Size(86, 17)
        Me.chk4.TabIndex = 136
        Me.chk4.Text = "Jerarqu�as"
        Me.chk4.UseVisualStyleBackColor = True
        '
        'chk5
        '
        Me.chk5.AutoSize = True
        Me.chk5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chk5.Location = New System.Drawing.Point(240, 355)
        Me.chk5.Name = "chk5"
        Me.chk5.Size = New System.Drawing.Size(71, 17)
        Me.chk5.TabIndex = 137
        Me.chk5.Text = "Divisi�n"
        Me.chk5.UseVisualStyleBackColor = True
        '
        'chk6
        '
        Me.chk6.AutoSize = True
        Me.chk6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chk6.Location = New System.Drawing.Point(397, 263)
        Me.chk6.Name = "chk6"
        Me.chk6.Size = New System.Drawing.Size(178, 17)
        Me.chk6.TabIndex = 138
        Me.chk6.Text = "Cobranza meses anteriores"
        Me.chk6.UseVisualStyleBackColor = True
        '
        'chk7
        '
        Me.chk7.AutoSize = True
        Me.chk7.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chk7.Location = New System.Drawing.Point(397, 286)
        Me.chk7.Name = "chk7"
        Me.chk7.Size = New System.Drawing.Size(144, 17)
        Me.chk7.TabIndex = 139
        Me.chk7.Text = "Cobranza mes actual"
        Me.chk7.UseVisualStyleBackColor = True
        '
        'chkCierre
        '
        Me.chkCierre.AutoSize = True
        Me.chkCierre.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkCierre.Location = New System.Drawing.Point(397, 309)
        Me.chkCierre.Name = "chkCierre"
        Me.chkCierre.Size = New System.Drawing.Size(59, 17)
        Me.chkCierre.TabIndex = 140
        Me.chkCierre.Text = "Cierre"
        Me.chkCierre.UseVisualStyleBackColor = True
        '
        'chkSeleccionar
        '
        Me.chkSeleccionar.AutoSize = True
        Me.chkSeleccionar.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkSeleccionar.Location = New System.Drawing.Point(17, 295)
        Me.chkSeleccionar.Name = "chkSeleccionar"
        Me.chkSeleccionar.Size = New System.Drawing.Size(180, 17)
        Me.chkSeleccionar.TabIndex = 141
        Me.chkSeleccionar.Text = "Seleccionar/Deseleccionar"
        Me.chkSeleccionar.UseVisualStyleBackColor = True
        '
        'jsVenProCierreDiario
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(234, Byte), Integer), CType(CType(241, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(732, 488)
        Me.ControlBox = False
        Me.Controls.Add(Me.chkSeleccionar)
        Me.Controls.Add(Me.chkCierre)
        Me.Controls.Add(Me.chk7)
        Me.Controls.Add(Me.chk6)
        Me.Controls.Add(Me.chk5)
        Me.Controls.Add(Me.chk4)
        Me.Controls.Add(Me.chk3)
        Me.Controls.Add(Me.chk2)
        Me.Controls.Add(Me.chk1)
        Me.Controls.Add(Me.btnFecha)
        Me.Controls.Add(Me.txtFechaProceso)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.grpAceptarSalir)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.C1PictureBox1)
        Me.Controls.Add(Me.lblInfo)
        Me.Controls.Add(Me.grpCaja)
        Me.Controls.Add(Me.grpTotales)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "jsVenProCierreDiario"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Tag = "Cierre diario"
        Me.grpCaja.ResumeLayout(False)
        Me.grpTotales.ResumeLayout(False)
        Me.grpAceptarSalir.ResumeLayout(False)
        CType(Me.C1PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents grpCaja As System.Windows.Forms.GroupBox
    Friend WithEvents grpTotales As System.Windows.Forms.GroupBox
    Friend WithEvents grpAceptarSalir As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents C1PictureBox1 As C1.Win.C1Input.C1PictureBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents lblLeyenda As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblProgreso As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtFechaProceso As System.Windows.Forms.TextBox
    Friend WithEvents btnFecha As System.Windows.Forms.Button
    Friend WithEvents chk1 As System.Windows.Forms.CheckBox
    Friend WithEvents chk2 As System.Windows.Forms.CheckBox
    Friend WithEvents chk3 As System.Windows.Forms.CheckBox
    Friend WithEvents chk4 As System.Windows.Forms.CheckBox
    Friend WithEvents chk5 As System.Windows.Forms.CheckBox
    Friend WithEvents chk6 As System.Windows.Forms.CheckBox
    Friend WithEvents chk7 As System.Windows.Forms.CheckBox
    Friend WithEvents chkCierre As System.Windows.Forms.CheckBox
    Friend WithEvents chkSeleccionar As System.Windows.Forms.CheckBox
End Class

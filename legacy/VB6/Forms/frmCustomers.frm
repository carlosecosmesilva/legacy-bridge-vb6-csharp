VERSION 5.00
Object = "{6B7E6392-850A-101B-AFC0-4210102A8DA7}#1.3#0"; "COMCTL32.OCX"
Begin VB.Form frmCustomers 
   Caption         =   "Customers"
   ClientHeight    =   6360
   ClientLeft      =   6420
   ClientTop       =   2415
   ClientWidth     =   8820
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   ScaleHeight     =   6360
   ScaleWidth      =   8820
   Begin VB.TextBox txtSearch 
      Height          =   375
      Left            =   120
      TabIndex        =   2
      ToolTipText     =   "Texto para busca de clientes"
      Top             =   600
      Width           =   6135
   End
   Begin ComctlLib.ListView lvCustomers 
      Height          =   4935
      Left            =   120
      TabIndex        =   1
      Top             =   1080
      Width           =   8535
      _ExtentX        =   15055
      _ExtentY        =   8705
      LabelWrap       =   -1  'True
      HideSelection   =   -1  'True
      _Version        =   327682
      ForeColor       =   -2147483640
      BackColor       =   -2147483643
      BorderStyle     =   1
      Appearance      =   1
      NumItems        =   0
   End
   Begin VB.CommandButton cmdSearch 
      Caption         =   "&Procurar"
      Height          =   735
      Left            =   6840
      TabIndex        =   0
      Top             =   240
      Width           =   1815
   End
   Begin VB.Label lblNome 
      BackStyle       =   0  'Transparent
      Caption         =   "Nome"
      BeginProperty Font 
         Name            =   "MS Sans Serif"
         Size            =   12
         Charset         =   0
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   375
      Left            =   120
      TabIndex        =   3
      Top             =   240
      Width           =   1935
   End
End
Attribute VB_Name = "frmCustomers"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private Sub cmdSearch_Click()
    Call SearchCustomers
End Sub

Private Sub cmdSearch_KeyPress(KeyAscii As Integer)
    If KeyAscii = vbKeyReturn Then
        Call SearchCustomers
    End If
End Sub

Private Sub Form_Load()
    SetupListView lvCustomers, EndpointClientes
End Sub

Private Sub Form_Unload(Cancel As Integer)
    txtSearch.Text = ""
End Sub

Private Sub txtSearch_KeyPress(KeyAscii As Integer)
    If KeyAscii = vbKeyReturn Then
        cmdSearch.SetFocus
    End If
End Sub

Private Function SearchAll() As Boolean
    Dim result As VbMsgBoxResult
    result = MsgBox("Deseja pesquisar todos os clientes?", vbQuestion + vbYesNo, "Search All")
    If result = VbMsgBoxResult.vbNo Then
        txtSearch.SetFocus
        Exit Function
    End If
    SearchAll = True
End Function

Private Sub SearchCustomers()
    Dim termo As String, erro As String
    termo = Trim(txtSearch.Text)

    If termo = "" Then
        If Not SearchAll() Then
            MsgBox "Digite um nome parcial para buscar.", vbExclamation
            Exit Sub
        End If
    End If
    If Not SearchApi(lvCustomers, EndpointClientes, erro, termo) Then
        MsgBox erro, vbExclamation, "Customers"
    End If
End Sub

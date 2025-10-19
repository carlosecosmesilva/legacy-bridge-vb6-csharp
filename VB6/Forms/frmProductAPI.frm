VERSION 5.00
Object = "{6B7E6392-850A-101B-AFC0-4210102A8DA7}#1.3#0"; "COMCTL32.OCX"
Begin VB.Form frmProductAPI 
   Caption         =   "Consultar Produtos - API"
   ClientHeight    =   6375
   ClientLeft      =   6405
   ClientTop       =   2415
   ClientWidth     =   8835
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   ScaleHeight     =   6375
   ScaleWidth      =   8835
   Begin ComctlLib.ListView lvProducts 
      Height          =   4815
      Left            =   120
      TabIndex        =   1
      Top             =   1200
      Width           =   8535
      _ExtentX        =   15055
      _ExtentY        =   8493
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
      Height          =   615
      Left            =   6480
      TabIndex        =   0
      Top             =   360
      Width           =   2175
   End
End
Attribute VB_Name = "frmProductAPI"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Private Sub cmdSearch_KeyPress(KeyAscii As Integer)
    Call RefreshProducts
End Sub

Private Sub Form_Load()
    SetupListView lvProducts, EndpointProdutos
End Sub

Private Sub cmdSearch_Click()
    cmdSearch.Enabled = False
    Call RefreshProducts
    cmdSearch.Enabled = True
End Sub

Private Sub RefreshProducts()
    Dim erro As String
    
    If Not SearchApi(lvProducts, EndpointProdutos, erro) Then
        MsgBox erro, vbExclamation
    End If
End Sub

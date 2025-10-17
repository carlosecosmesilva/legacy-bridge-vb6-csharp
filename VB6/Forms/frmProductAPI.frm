VERSION 5.00
Object = "{6B7E6392-850A-101B-AFC0-4210102A8DA7}#1.3#0"; "COMCTL32.OCX"
Begin VB.Form frmProductAPI 
   Caption         =   "Product"
   ClientHeight    =   4770
   ClientLeft      =   120
   ClientTop       =   465
   ClientWidth     =   6945
   LinkTopic       =   "frmProductAPI"
   MaxButton       =   0   'False
   ScaleHeight     =   4770
   ScaleWidth      =   6945
   StartUpPosition =   2  'CenterScreen
   Begin ComctlLib.ListView lvProducts 
      Height          =   3615
      Left            =   120
      TabIndex        =   1
      Top             =   960
      Width           =   6615
      _ExtentX        =   11668
      _ExtentY        =   6376
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
      Left            =   5160
      TabIndex        =   0
      ToolTipText     =   "Efetua request na API"
      Top             =   240
      Width           =   1575
   End
End
Attribute VB_Name = "frmProductAPI"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Private Sub cmdSearch_Click()
    LoadProductsIntoListView lvProducts
End Sub

Private Sub SetupListView()
    With lvProducts
        .ColumnHeaders.Clear
        .ColumnHeaders.Add , , "Produto", 2000
        .ColumnHeaders.Add , , "Preço (R$)", 1500
        .View = lvwReport
'        .FullRowSelect = True
'        .GridLines = True
    End With
End Sub

Private Sub Form_Load()
    Call SetupListView
End Sub

Attribute VB_Name = "ApiConsumer"
Public Sub LoadProducts()
    Dim xhr As Object
    Set xhr = CreateObject("MSXML2.XMLHTTP.6.0")
    xhr.Open "GET", "http://localhost:5000/api/products", False
    xhr.Send
    MsgBox xhr.responseText
End Sub

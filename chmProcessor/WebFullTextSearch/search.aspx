<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="search.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>%Search%</title>
</head>
<body>
%HEADER%
<h1>%Search Result%</h1>
    <form id="form1" runat="server">
    <table width="100%" border="0" ><tr><td><b>%Search text%:</b><asp:Label ID="txtSearchText" runat="server" Text="txtSearchText"></asp:Label></td><td align="right"><b>%Showing results%
        <asp:Label ID="txtShowResults" runat="server" Text="txtShowResults"></asp:Label>
        %of% 
        <asp:Label ID="txtTotalResults" runat="server" Text="txtTotalResults"></asp:Label></b></td></tr></table>
    <div>
        <asp:Literal ID="txtResult" runat="server"></asp:Literal></div>
        <p align="center"><b>
            <asp:Label ID="txtMoreResults" runat="server" Text="%More results%"></asp:Label></b><br/>
            <asp:HyperLink ID="lnkPrevious" runat="server">%Previous%</asp:HyperLink> 
            <asp:Literal ID="txtResultLinks" runat="server"></asp:Literal>
            <asp:HyperLink ID="lnkNext" runat="server">%Next%</asp:HyperLink></p>
        <p><b>
            %Search time%:
            <asp:Label ID="txtMiliseconds" runat="server" Text="txtMiliseconds"></asp:Label>
            %miliseconds%</b></p>
    </form>
    
%FOOTER%
</body>
</html>

﻿<%@ Control Language="C#" CodeBehind="Enumeration_Edit.ascx.cs" Inherits="DynamicDataWebSite.Enumeration_EditField" %>

<asp:DropDownList ID="DropDownList1" runat="server" CssClass="DDDropDown" OnDataBound="DropDownList1_DataBound" />

<asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator1" CssClass="DDControl DDValidator" ControlToValidate="DropDownList1" Display="Dynamic" Enabled="false" />
<asp:DynamicValidator runat="server" ID="DynamicValidator1" CssClass="DDControl DDValidator" ControlToValidate="DropDownList1" Display="Dynamic" />


﻿using DynamicDataLibrary;
using DynamicDataLibrary.Attributes;
using DynamicDataLibrary.ModelRelated;
using System;
using System.Web.DynamicData;
using System.Web.UI.WebControls;
using NotAClue.Web.DynamicData;
using System.Collections.Specialized;
using DynamicDataModel.Model;
using System.Linq;

using EntityDataSourceChangingEventArgs = Microsoft.AspNet.EntityDataSource.EntityDataSourceChangingEventArgs;
using EntityDataSourceChangedEventArgs = Microsoft.AspNet.EntityDataSource.EntityDataSourceChangedEventArgs;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace DynamicDataWebSite.DynamicData.CustomPages.Properties
{
    public partial class Details : System.Web.UI.Page
    {
        private OrderedDictionary backUpKeys = new OrderedDictionary();
        protected bool ForceDelete { get; set; }
        protected MetaTable table;

        public string DeleteItemCommandText { get; set; }
        public string DeleteConfirmationMesssage { get; set; }
        public string EditItemCommandText { get; set; }
        public string ItemDetailsTitle { get; set; }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            bool isPrint = false;
            if (Boolean.TryParse(this.Request.Params["Print"], out isPrint) && isPrint)
                this.MasterPageFile = "~/Print.Master";
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            table = DynamicDataRouteHandler.GetRequestMetaTable(Context);
            if (table != null)
                FormView1.SetMetaTable(table);
            else
                Response.Redirect("~/");
            DetailsDataSource.EntityTypeFilter = table.EntityType.Name;
            SetText();
            // Add meta description tag
            HtmlMeta metaDescription = new HtmlMeta();
            metaDescription.Name = Resources.RealEstate.Page_Details_Title;
            metaDescription.Content = Resources.RealEstate.Page_Desc_Desc;
            Page.Header.Controls.Add(metaDescription);

            // Add meta keywords tag
            HtmlMeta metaKeywords = new HtmlMeta();
            metaKeywords.Name = Resources.RealEstate.Page_Desc_Desc;
            metaKeywords.Content = Page.Title+ ","+ Resources.RealEstate.Page_Desc_Desc;
            Page.Header.Controls.Add(metaKeywords);
        }

        private void SetText()
        {
            DynamicDataEntityCustomTextAttribute customText = table.Attributes.GetAttribute<DynamicDataEntityCustomTextAttribute>();
            if (customText != null)
            {
                this.EditItemCommandText = customText.EditItemCommandTextAtList;
                this.DeleteItemCommandText = customText.DeleteItemCommandText;
                if (customText.ItemDetailsTitleFormat != null)
                    this.ItemDetailsTitle = String.Format(customText.ItemDetailsTitleFormat, table.DisplayName);
                this.DeleteConfirmationMesssage = customText.DeleteConfirmationMesssage;
            }
            if (String.IsNullOrEmpty(this.ItemDetailsTitle))
                this.ItemDetailsTitle = String.Format(Convert.ToString(GetGlobalResourceObject("DynamicData", "ItemDetailsTitleFormat")), table.DisplayName);
            if (String.IsNullOrEmpty(this.EditItemCommandText))
                this.EditItemCommandText = Convert.ToString(GetGlobalResourceObject("DynamicData", "Update"));
            if (String.IsNullOrEmpty(this.DeleteItemCommandText))
                this.DeleteItemCommandText = Convert.ToString(GetGlobalResourceObject("DynamicData", "Delete"));
            if (String.IsNullOrEmpty(this.DeleteConfirmationMesssage))
                this.DeleteConfirmationMesssage = Convert.ToString(GetGlobalResourceObject("DynamicData", "DelConfirm"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string title = GetGlobalResourceObject("RealEstate", "Page_Details_Title") as string;
            Title = title != "" ? title : table.DisplayName;
            
            DetailsDataSource.Include = table.ForeignKeyColumnsNames;
            ListHyperLink.NavigateUrl = table.GetActionPath("ListWithIcons");
            ListHyperLink.Text = Resources.RealEstate.AllRecords;
            List<DynamicDataModel.Model.Property> ls = DynamicDataHelper.GetListOfProperties(8);
            try
            {
                if (FormView1.CurrentMode == FormViewMode.ReadOnly)
                {

                    var fet = FormView1.FindControl("fetrep") as Repeater;
                    //    var fut = FormView1.FindControl("futrep") as Repeater;
                    fet.DataSource = ls;
                    fet.DataBind();
                }
            //    int mx = ls.Count - 5;
            //    for (int i = 0; i < mx; i++)
            //    {
            //        ls.Remove(ls[i]);
            //    }
            //    fut.DataSource = ls;
            //    fut.DataBind();
            }
            catch
            {

            //    throw;
            }
        }

        protected void DetailsDataSource_QueryCreated(object sender, QueryCreatedEventArgs e)
        {
            if (Entities.CustomQuery.ContainsKey(table.EntityType))
            {
                e.Query = Entities.CustomQuery[table.EntityType]
                    .Invoke(e.Query);
            }
            else
            {
                ViewLimitedToCurrentUserAttribute dataLimitedToCurrentUser = table.GetAttribute<ViewLimitedToCurrentUserAttribute>();
                if (dataLimitedToCurrentUser != null)
                {
                    Nullable<int> numberOfItems;
                    dataLimitedToCurrentUser.Handle(e, this.table, this.Page, out numberOfItems);
                    if (numberOfItems == 1)
                        this.ListHyperLink.Visible = false;
                }
            }
        }

        protected void FormView1_ItemDeleting(object sender, FormViewDeleteEventArgs e)
        {
            bool hasInsertPermission = DynamicDataLibrary.AuthorizationManager.CheckUrlAccessForPrincipal(table.GetActionPath(PageAction.Insert), this.Page.User);
            if (!hasInsertPermission)
            {
                Response.Redirect(SecurityHandler.ErrorPath);
            }

            this.table.Copy(e.Keys, this.backUpKeys);

            if (Entities.CustomCheckBeforeDelete.ContainsKey(table.EntityType))
            {
                string message;
                MessageActionType messageActionType;
                bool isValueFilledManually = false;
                if (e.Values.Count == 0)
                {
                    this.table.FillValues(e.Keys, e.Values);
                    isValueFilledManually = true;
                }
                bool canDelete = Entities.CustomCheckBeforeDelete[table.EntityType]
                    .Invoke(e.Values, ForceDelete, out message, out messageActionType);
                if (isValueFilledManually)
                    e.Values.Clear();

                e.Cancel = !canDelete;
                if (messageActionType == MessageActionType.NeedConfirmation)
                {
                    this.LabelConfirmationMessage.Text = message;
                    this.ModalPopupExtenderConfirmationMessage.Show();
                }
                else if (messageActionType == MessageActionType.Error)
                {
                    this.ShowErrorMessage(message);
                }
                else if (messageActionType == MessageActionType.Information)
                {
                    this.LabelInformationMessage.Text = message;
                    this.ModalPopupExtenderInformationMessage.Show();
                }
            }
        }

        protected void FormView1_ItemDeleted(object sender, FormViewDeletedEventArgs e)
        {
            if (e.Exception == null || e.ExceptionHandled)
            {
                //Handle custom actions if any:
                string message = null;
                MessageAfterActionType messageActionType = MessageAfterActionType.NoMessage;
                if (Entities.CustomProcessAfterDelete.ContainsKey(table.EntityType))
                {
                    bool isValueFilledManually = false;
                    if (e.Keys.Count == 0)
                    {
                        this.table.Copy(this.backUpKeys, e.Keys);
                    }
                    if (e.Values.Count == 0)
                    {
                        this.table.FillValues(e.Keys, e.Values);
                        isValueFilledManually = true;
                    }
                    Entities.CustomProcessAfterDelete[table.EntityType]
                        .Invoke(e.Values, e.AffectedRows, out message, out messageActionType);
                    if (isValueFilledManually)
                        e.Values.Clear();
                    if (messageActionType == MessageAfterActionType.Error)
                    {
                        this.ShowErrorMessage(message);
                        return;
                    }
                    else if (messageActionType == MessageAfterActionType.Information)
                    {
                        this.ShowInfoMessage(message);
                        return;
                    }
                }
                Response.Redirect(table.ListActionPath);
            }
            else
            {
                e.ExceptionHandled = true;
                //this.FormView1.Visible = false;
                if (e.Exception.HResult == -2146233087)
                    ShowErrorMessage(Convert.ToString(GetGlobalResourceObject("DynamicData", "Error_CannotDeleteBecauseOfReletedRecords")));
                else
                    ShowErrorMessage(String.Format("{0}:<br/>{1}", GetGlobalResourceObject("DynamicData", "Error_CannotDeleteBecauseOf"),
                        e.Exception.Message));
            }
        }

        protected void LinkButtonConfirmationDelete_Click(object sender, EventArgs e)
        {
            this.ForceDelete = true;
            this.FormView1.DeleteItem();
        }

        protected void LinkButtonInformationMessageOK_Click(object sender, EventArgs e)
        {
            this.FormView1.DataBind();
        }

        protected void LinkButtonErrorMessageOK_Click(object sender, EventArgs e)
        {
            this.FormView1.DataBind();
        }

        protected void LinkButtonConfirmationCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(table.GetActionPath("ListWithIcons"));
        }

        protected void FormView1_DataBound(object sender, EventArgs e)
        {
            this.FormView1.HandleDependant(this.table);


            ContributionLimitedToCreatorAttribute contributionLimitedToCreator
                = table.GetAttribute<ContributionLimitedToCreatorAttribute>();
            if (contributionLimitedToCreator != null)
            {
                if (!contributionLimitedToCreator.CanContributeOnDataItem(this.FormView1.DataItem))
                {
                    this.FormView1.FindControl("DynamicHyperLink_Edit").Visible = false;
                    this.FormView1.FindControl("DynamicHyperLink_Delete").Visible = false;

                }
            }
            if (this.FormView1.DataItem != null)
            {
                object item = EntityDataSourceHelper.GetItemObject(this.FormView1.DataItem);
                foreach (DisableDeleteIfEqualAttribute att in table.Attributes.OfType<DisableDeleteIfEqualAttribute>())
                {
                    if (att.Values.Contains(item.GetType().GetProperty(att.PropertyName).GetValue(item)))
                    {
                        this.FormView1.FindControl("DynamicHyperLink_Delete").Visible = false;
                        break;
                    }
                }
            }
        }

        protected void DetailsDataSource_Deleting(object sender, EntityDataSourceChangingEventArgs e)
        {

        }

        protected void DetailsDataSource_Deleted(object sender, EntityDataSourceChangedEventArgs e)
        {

        }
        private void ShowErrorMessage(string message)
        {
            this.LabelErrorMessage.Text = message;
            this.ModalPopupExtenderErrorMessage.Show();
        }

        private void ShowInfoMessage(string message)
        {
            this.LabelInformationMessage.Text = message;
            this.ModalPopupExtenderInformationMessage.Show();
        }

        private void ShowConfirmationMessage(string message)
        {
            this.LabelConfirmationMessage.Text = message;
            this.ModalPopupExtenderConfirmationMessage.Show();
        }
        protected string GetText(object dataItem, string colName)
        {
            string col = "";
            bool conf = false;
            switch (colName)
            {
                case "DescTxt":
                    col = "Property_Type_Description";
                    break;
                case "ImgUrl":
                    col = "Property_Photo";
                    conf = true;
                    break;
                case "NavUrl":
                    col = "PropertyID";
                    conf = true;
                    break;
            }
            string val = "";
            if (colName == "DescTxt")
            {
                conf = true;
                val = Convert.ToString(DataBinder.Eval(((Property)dataItem).Property_Type, col));
            }
            else
                val = Convert.ToString(DataBinder.Eval(dataItem, col));
            if (!string.IsNullOrEmpty(val) && conf == false)
            {
                return GetGlobalResourceObject("Property_Type", val).ToString();
            }
            else
            {
                if (colName == "NavUrl")
                {
                    return Global.DefaultModel.GetActionPath(table.Name, "DetailsWithIcons", null) + "?PropertyID=" + val;
                }
            }
            return val;
        }

        protected void FormView1_ItemCommand(object sender, FormViewCommandEventArgs e)
        {
            if (e.CommandName == DataControlCommands.CancelCommandName)
            {
                Response.Redirect(table.GetActionPath("ListWithIcons"));
            }
        }

        protected void Dynamic_DataBinding(object sender, EventArgs e)
        {
        }
    }
}
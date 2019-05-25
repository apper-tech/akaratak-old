//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DynamicDataModel.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Property
    {
        public int PropertyID { get; set; }
        public int Property_Type_ID { get; set; }
        public int Property_Size { get; set; }
        public System.DateTime Date_Added { get; set; }
        public int Floor { get; set; }
        public Nullable<bool> Has_Garage { get; set; }
        public Nullable<bool> Has_Garden { get; set; }
        public int Num_Bedrooms { get; set; }
        public int Num_Bathrooms { get; set; }
        public System.DateTime Expire_Date { get; set; }
        public Nullable<int> Contract_Type { get; set; }
        public Nullable<int> City_ID { get; set; }
        public Nullable<int> Country_ID { get; set; }
        public string Address { get; set; }
        public string Location { get; set; }
        public string Zip_Code { get; set; }
        public string Other_Details { get; set; }
        public Nullable<int> Sale_Price { get; set; }
        public Nullable<int> Rent_Price { get; set; }
        public Nullable<int> Num_Floors { get; set; }
        public string User_ID { get; set; }
        public string Property_Photo { get; set; }
        public int Property_Category_ID { get; set; }
        public string Property_Id_ext { get; set; }
        public string Url_ext { get; set; }
    
        public virtual City City { get; set; }
        public virtual Country Country { get; set; }
        public virtual User User { get; set; }
        public virtual Property_Category Property_Category { get; set; }
        public virtual Property_Type Property_Type { get; set; }
        public virtual Contract_Type Contract_Type1 { get; set; }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PayrollAttendance
{
    public partial class Attendance : System.Web.UI.Page
    {
        private const string apiKey = "8BYkEfBA6O6donzWlSihBXox7C0sKR6b";
        private const string link = "https://csms-rest-api.onrender.com";
        protected void Page_Load(object sender, EventArgs e)
        {
            txtCurrentDateTime.Text = DateTime.Now.ToString("MMMM dd, yyyy h:mm:ss tt");
        }
        protected async void Button1_Click(object sender, EventArgs e)
        {
            string Username = txtemail.Text.Trim();
            string Password = txtpassword.Text.Trim();
            try
            {

                using (var clientAllAttendance = new HttpClient())
                {
                    clientAllAttendance.BaseAddress = new Uri(link);
                    clientAllAttendance.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    var responseAllAttendance = await clientAllAttendance.GetAsync("attendance/all");
                    responseAllAttendance.EnsureSuccessStatusCode();

                    string allAttendanceJson = await responseAllAttendance.Content.ReadAsStringAsync();
                    dynamic allAttendanceData = Newtonsoft.Json.JsonConvert.DeserializeObject(allAttendanceJson);


                    if (allAttendanceData != null && allAttendanceData.employees != null)
                    {
                        foreach (var employee in allAttendanceData.employees)
                        {
                            if (employee != null &&
                                employee.email == Username)
                            {

                                if (employee.attendance != null)
                                {
                                    foreach (var attendanceRecord in employee.attendance)
                                    {
                                        if (attendanceRecord != null &&
                                            attendanceRecord.work_date == DateTime.UtcNow.Date.ToString("R") &&
                                            attendanceRecord.logout_status == null)
                                        {

                                            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('Email is already login','This email already have an attendance','warning');", true);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('No attendance records found for the user.');", true);
                    }
                }


                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(link);
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    var collection = new List<KeyValuePair<string, string>>();
                    collection.Add(new KeyValuePair<string, string>("email", Username));
                    collection.Add(new KeyValuePair<string, string>("password", Password));
                    var content = new FormUrlEncodedContent(collection);

                    var response = await client.PostAsync("attendance", content);

                    if (response.IsSuccessStatusCode)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('Thank you!', 'Your attendance has been recorded', 'success');", true);
                        txtemail.Text = "";
                        txtpassword.Text = "";
                    }
                    else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlertError", $"swal('Error!', 'Internal Server Error', 'error');", true);
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('Email is not registered!', 'Please contact admin to verify your email', 'error');", true);

                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        dynamic errorJson = Newtonsoft.Json.JsonConvert.DeserializeObject(errorContent);
                        string errorMessage = errorJson?.error?.message ?? "Unknown error";

                        if (errorMessage.Contains("Invalid credentials"))
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('Incorrect Password!', 'Please enter the correct password', 'error');", true);
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlertError", $"swal('Error', '{errorMessage}', 'error');", true);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlertError", $"swal('Error!', 'An error occurred: {ex.Message}', 'error');", true);
            }
        }

        protected async void Button2_Click(object sender, EventArgs e)
        {
            string Username = txtemail.Text.Trim();
            string Password = txtpassword.Text.Trim();


            try
            {
                using (var clientAllAttendance = new HttpClient())
                {
                    clientAllAttendance.BaseAddress = new Uri(link);
                    clientAllAttendance.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    var responseAllAttendance = await clientAllAttendance.GetAsync("attendance/all");
                    responseAllAttendance.EnsureSuccessStatusCode();

                    string allAttendanceJson = await responseAllAttendance.Content.ReadAsStringAsync();
                    dynamic allAttendanceData = Newtonsoft.Json.JsonConvert.DeserializeObject(allAttendanceJson);

                    if (allAttendanceData != null && allAttendanceData.employees != null)
                    {
                        foreach (var employee in allAttendanceData.employees)
                        {
                            if (employee != null && employee.email == Username)
                            {
                                if (employee.attendance == null || !HasAttendanceToday(employee.attendance))
                                {
                                    ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('Email is not yet login','Please make a record of attendance first','warning');", true);
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('No attendance records found for the user.');", true);
                    }
                }


                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(link);
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    var collection = new List<KeyValuePair<string, string>>();
                    collection.Add(new KeyValuePair<string, string>("email", Username));
                    collection.Add(new KeyValuePair<string, string>("password", Password));
                    var content = new FormUrlEncodedContent(collection);

                    var response = await client.PostAsync("attendance", content);

                    if (response.IsSuccessStatusCode)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('Thank you!', 'Your account successfully logout', 'success');", true);
                        txtemail.Text = "";
                        txtpassword.Text = "";
                    }
                    else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlertError", $"swal('Error!', 'Internal Server Error', 'error');", true);
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {

                        ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('Email is not registered!', 'Please contact admin to verify your email', 'error');", true);

                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        dynamic errorJson = Newtonsoft.Json.JsonConvert.DeserializeObject(errorContent);
                        string errorMessage = errorJson?.error?.message ?? "Unknown error";

                        if (errorMessage.Contains("Invalid credentials"))
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlert", $"swal('Incorrect Password!', 'Please enter the correct password', 'error');", true);
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlertError", $"swal('Error', '{errorMessage}', 'error');", true);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "SweetAlertError", $"swal('Error!', 'An error occurred: {ex.Message}', 'error');", true);
            }
        }


        private bool HasAttendanceToday(dynamic attendanceRecords)
        {
            foreach (var record in attendanceRecords)
            {
                DateTime recordDate = DateTime.Parse(record.work_date.ToString());
                if (recordDate.Date == DateTime.UtcNow.Date)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace TokenEmailPush
{   
    public class ApplicationController
    {
        private Thread _pollingThread = null;
        private Thread _pollingThreadStaging = null;
        private Thread _pushThread = null;
        private Queue<Email> _emailsQueue =  new Queue<Email>();
        private AmazonSimpleEmailServiceClient _sesClient = new AmazonSimpleEmailServiceClient(region: Amazon.RegionEndpoint.USEast1);
        public ApplicationController()
        {
            this.Configure();
        }
        public void Start()
        {
            this._pollingThread = new Thread(new ParameterizedThreadStart(this.StartPolling));
            this._pollingThread.Name = "Poller";
            this._pollingThread.Start(Environment.STAGING);
            this._pushThread = new Thread(new ThreadStart(this.StartPushing));
            this._pushThread.Name = "SESClient";
            this._pushThread.Start();
            //if (SharedClass.PollStaging)
            //{
            //    this._pollingThreadStaging = new Thread(new ParameterizedThreadStart(this.StartPolling));
            //    this._pollingThreadStaging.Name = "PollerStaging";
            //    this._pollingThreadStaging.Start();
            //}
        }
        private void StartPolling(object input)
        {
            Environment environment = Environment.STAGING;
            SqlConnection sqlCon = new SqlConnection(SharedClass.GetConnectionString(environment));
            SqlCommand sqlCmd = new SqlCommand("Get_Pending_Token_Emails", sqlCon);
            SqlDataAdapter da = null;
            DataSet ds = null;
            sqlCmd.CommandType = CommandType.StoredProcedure;
            while (!SharedClass.HasStopSignal)
            {
                try
                {
                    sqlCmd.Parameters.Clear();
                    sqlCmd.Parameters.Add("@LastId", SqlDbType.BigInt).Value = SharedClass.QueueSlno.GetSlno(environment);
                    if (sqlCon.State != ConnectionState.Open)
                        sqlCon.Open();
                    da = new SqlDataAdapter();
                    da.SelectCommand = sqlCmd;
                    ds = new DataSet();
                    da.Fill(ds);
                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        SharedClass.QueueSlno.SetSlno(environment, Convert.ToInt64( ds.Tables[0].Compute("Max(Id)", "")));
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            Email email = new Email();
                            email.Environment = environment;
                            email.Id = Convert.ToInt64(row["Id"].ToString());
                            email.TokenId = Convert.ToInt64(row["TokenId"].ToString());
                            if (!row["TokenConversationId"].IsDBNull())
                                email.TokenConversationId = Convert.ToInt64(row["TokenConversationId"].ToString());
                            if (!row["FromAddress"].IsDBNull())
                                email.FromAddress = row["FromAddress"].ToString();
                            if (!row["FromDisplay"].IsDBNull())
                                email.FromDisplay = row["FromDisplay"].ToString();
                            foreach (string toAddress in row["ToAddresses"].ToString().Split(','))
                                email.ToAddresses.Add(toAddress);
                            if (!row["CCAddresses"].IsDBNull() && (row["CCAddresses"].ToString() != "" && row["CCAddresses"].ToString() != "null"))
                                foreach (string ccAddress in row["CCAddresses"].ToString().Split(','))
                                    email.CCAddresses.Add(ccAddress);
                            if (!row["BCCAddresses"].IsDBNull() && (row["BCCAddresses"].ToString() != "" && row["BCCAddresses"].ToString() != "null"))
                                foreach (string bccAddress in row["BCCAddresses"].ToString().Split(','))
                                    email.BCCAddresses.Add(bccAddress);
                            email.Body = row["Body"].ToString();
                            email.Subject = row["Subject"].ToString();
                            if (!row["ReplyToAddresses"].IsDBNull())
                                foreach (string replyToAddress in row["ReplyToAddresses"].ToString().Split(','))
                                    email.ReplyToAddresses.Add(replyToAddress);
                            if (!row["ReturnPath"].IsDBNull())
                                email.ReturnPath = row["ReturnPath"].ToString();
                            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                            {
                                foreach (DataRow attachmentRow in ds.Tables[1].Select("Id = " + Convert.ToInt64(email.Id)))
                                {
                                    Attachment attachment = new Attachment();
                                    attachment.Name = attachmentRow["Name"].ToString();
                                    attachment.Path = attachmentRow["Path"].ToString();
                                    email.Attachments.Add(attachment);
                                }
                            }
                            this.EnQueue(email);
                        }
                    }
                    else
                    {
                        try
                        {
                            Thread.Sleep(5000);
                        }
                        catch (ThreadInterruptedException e)
                        { }
                        catch (ThreadAbortException e)
                        { }
                    }
                }
                catch (Exception e)
                {
                    SharedClass.Logger.Error("Exception in Polling, " + e.ToString());
                }
                finally
                {
                    try
                    {
                        da.Dispose();
                    }
                    catch (NullReferenceException e)
                    { }
                    try
                    {
                        ds.Dispose();
                    }
                    catch (NullReferenceException e)
                    { }
                }
            }
        }
        private void StartPushing()
        {   
            Email email = null;
            SendRawEmailRequest request = null;
            SendRawEmailResponse response = null;
            RawMessage rawMessage = null;
            StringBuilder headers = null;
            StringBuilder attachments = null;
            while (!SharedClass.HasStopSignal)
            {
                try
                {
                    if (this.QueueCount() == 0)
                    {
                        Thread.Sleep(2000);
                        continue;
                    }
                    else
                    {
                        email = this.DeQueue();
                    }
                    //if ((email = this.DeQueue()) == null)
                    //{
                    //    Thread.Sleep(2000);
                    //    continue;
                    //}
                    headers = new StringBuilder();
                    headers.Append("From:");
                    if (email.FromDisplay.Length > 0)
                        attachments = null;
                        headers.Append(email.FromDisplay);
                        //headers.Append("\"" + email.FromDisplay + "\"");

                    headers.Append((email.FromAddress.Length > 0 ? "<"+email.FromAddress+">" : "<"+"noreply@smscountry.com"+">") + System.Environment.NewLine);
                    headers.Append("To:" + string.Join(",", email.ToAddresses) + System.Environment.NewLine);
                    headers.Append("Subject:" + email.Subject + System.Environment.NewLine);
                    if (email.CCAddresses.Count > 0)
                        headers.Append("Cc:" + string.Join(",", email.CCAddresses) + System.Environment.NewLine);
                    if (email.BCCAddresses.Count > 0)
                        headers.Append("Bcc:" + string.Join(",", email.BCCAddresses + System.Environment.NewLine));
                    if (email.ReplyToAddresses.Count > 0)
                        headers.Append("Reply-To:" + string.Join(",", email.ReplyToAddresses) + System.Environment.NewLine);
                    if (email.ReturnPath.Length > 0)
                        headers.Append("Return-Path:" + email.ReturnPath + System.Environment.NewLine);
                    if (email.InReplyTo.Count > 0)
                        headers.Append("In-Reply-To:" + string.Join(",", email.InReplyTo) + System.Environment.NewLine);
                    Console.WriteLine(headers);
                    if (email.Attachments.Count > 0)
                    {
                        attachments = new StringBuilder();
                        foreach (Attachment attachment in email.Attachments)
                        {
                            attachments.Append(System.Environment.NewLine + System.Environment.NewLine + "--" + email.Boundary + System.Environment.NewLine);
                            attachments.Append("Content-Type:" + attachment.ContentType + "; name=" + attachment.Name + System.Environment.NewLine);
                            attachments.Append("Content-Description:" + attachment.Name + System.Environment.NewLine);
                            attachments.Append("Content-Disposition: attachment; filename=" + attachment.Name + System.Environment.NewLine);
                            attachments.Append("Content-Transfer-Encoding:base64" + System.Environment.NewLine + System.Environment.NewLine);
                            attachments.Append(Convert.ToBase64String(System.IO.File.ReadAllBytes(SharedClass.TokenAttachmentsPath + attachment.Path)) + System.Environment.NewLine);
                            attachments.Append("--" + email.Boundary);
                        }
                        headers.Append("Content-Type:multipart/mixed;boundary=\"" + email.Boundary + "\"" + System.Environment.NewLine);
                        headers.Append("MIME-Version:1.0" + System.Environment.NewLine + System.Environment.NewLine);
                    }
                    headers.Append("Content-Type:text/html;" + System.Environment.NewLine + System.Environment.NewLine);
                    headers.Append(email.Body);
                    if (attachments != null)
                        headers.Append(attachments.ToString() + System.Environment.NewLine);
                    request = new SendRawEmailRequest();
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(headers.ToString()));
                    rawMessage = new RawMessage();
                    rawMessage.Data = stream;
                    request.RawMessage = rawMessage;
                    request.Destinations = email.ToAddresses;
                    if (email.FromDisplay.Length > 0)
                        request.Source += @"\" + email.FromDisplay + @"\";
                    
                    request.Source += "<" + email.FromAddress + ">";
                    response = _sesClient.SendRawEmail(request);
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        email.UpdateStatus(1, response.MessageId);
                    }
                    else
                    {
                        email.UpdateStatus(2, string.Empty);
                    }
                }
                catch (Exception e)
                {
                    SharedClass.Logger.Error(e.ToString());
                    email.UpdateStatus(2, string.Empty);
                }
            }
        }
        private void EnQueue(Email email)
        {
            lock (this._emailsQueue)
            {
                this._emailsQueue.Enqueue(email);
            }
        }
        private Email DeQueue()
        {
            lock (this._emailsQueue)
            {
                return this._emailsQueue.Dequeue();
            }
        }

        private int QueueCount()
        {
                lock (this._emailsQueue)
                {
                    return this._emailsQueue.Count;
                }
        }
        private void Configure()
        {
            SharedClass.SetConnectionString(Environment.PRODUCTION, System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            if (System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionStringStaging"] != null)
            {
                SharedClass.PollStaging = true;
                SharedClass.SetConnectionString(Environment.STAGING, System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionStringStaging"].ConnectionString);
            }
            SharedClass.TokenAttachmentsPath = System.Configuration.ConfigurationManager.AppSettings["TokenAttachmentsPath"];
        }

        public void StopService()
        {

        }
    }
}

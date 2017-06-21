using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenEmailPush
{
    public class Email
    {
        private long _id = 0;
        private long _tokenId = 0;
        private long _tokenConversationId = 0;
        private string _fromAddress = string.Empty;
        private string _fromDisplay = string.Empty;
        private List<string> _toAddresses = new List<string>();
        private List<string> _ccAddresses = new List<string>();
        private List<string> _bccAddresses = new List<string>();
        private string _body = string.Empty;
        private string _subject = string.Empty;
        private List<string> _replyToAddresses = new List<string>();
        private List<string> _inReplyTo = new List<string>();
        private string _returnPath = string.Empty;
        private Environment _environment = Environment.PRODUCTION;
        private List<Attachment> _attachments = new List<Attachment>();
        private string _boundary = Guid.NewGuid().ToString();
        public void UpdateStatus(byte status, string messageId)
        {
            System.Data.SqlClient.SqlConnection sqlCon = new System.Data.SqlClient.SqlConnection(SharedClass.GetConnectionString(this._environment));
            System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand("Update_Token_Email_Status", sqlCon);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@EmailId", System.Data.SqlDbType.BigInt).Value = this._id;
            sqlCmd.Parameters.Add("@Status", System.Data.SqlDbType.TinyInt).Value = status;
            sqlCmd.Parameters.Add("@MessageId", System.Data.SqlDbType.VarChar, 200).Value = messageId;
            sqlCmd.Parameters.Add("@TokenId", System.Data.SqlDbType.BigInt).Value = this._tokenId;
            sqlCmd.Parameters.Add("@TokenConversationId", System.Data.SqlDbType.BigInt).Value = this._tokenConversationId;
            sqlCmd.Parameters.Add("@Success", System.Data.SqlDbType.Bit).Direction = System.Data.ParameterDirection.Output;
            sqlCmd.Parameters.Add("@Message", System.Data.SqlDbType.VarChar, 1000).Direction = System.Data.ParameterDirection.Output;
            try
            {
                sqlCon.Open();
                sqlCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            { }
            finally
            {
                if (sqlCon.State == System.Data.ConnectionState.Open)
                    sqlCon.Close();
                try
                { sqlCon.Dispose(); }
                catch (NullReferenceException e)
                { }
                try
                { sqlCmd.Dispose(); }
                catch (NullReferenceException e)
                { }
            }
        }
        public long Id
        {
            get { return this._id; }
            set { this._id = value; }
        }
        public long TokenId
        {
            get { return this._tokenId; }
            set { this._tokenId = value; }
        }
        public long TokenConversationId
        {
            get { return this._tokenConversationId; }
            set { this._tokenConversationId = value; }
        }
        public string FromAddress
        {
            get { return this._fromAddress; }
            set { this._fromAddress = value; }
        }
        public string FromDisplay
        {
            get { return this._fromDisplay; }
            set { this._fromDisplay = value; }
        }
        public List<string> ToAddresses
        {
            get { return this._toAddresses; }
        }
        public List<string> CCAddresses
        {
            get { return this._ccAddresses; }
        }
        public List<string> BCCAddresses
        {
            get { return this._bccAddresses; }
        }
        public string Body
        {
            get { return this._body; }
            set { this._body = value; }
        }
        public string Subject
        {
            get { return this._subject; }
            set { this._subject = value; }
        }
        public List<string> ReplyToAddresses
        {
            get { return this._replyToAddresses; }
        }
        public List<string> InReplyTo
        {
            get { return this._inReplyTo; }
        }
        public string ReturnPath
        {
            get { return this._returnPath; }
            set { this._returnPath = value; }
        }
        public List<Attachment> Attachments
        {
            get { return this._attachments; }
        }
        public Environment Environment
        {
            get { return this._environment; }
            set { this._environment = value; }
        }
        public string Boundary
        {
            get { return this._boundary; }            
        }
    }
}

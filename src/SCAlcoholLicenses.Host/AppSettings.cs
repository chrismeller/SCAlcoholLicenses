namespace SCAlcoholLicenses.Host
{
	public class AppSettings
	{
        public class ProxyInfo
        {
			public string Hostname { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
        }

		public ProxyInfo Proxy { get; set; }
	}
}

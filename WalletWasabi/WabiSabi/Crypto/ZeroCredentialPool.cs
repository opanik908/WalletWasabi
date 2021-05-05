using System.Linq;
using System.Collections.Generic;
using WalletWasabi.Crypto.ZeroKnowledge;
using System.Collections.Concurrent;

namespace WalletWasabi.WabiSabi.Crypto
{
	/// <summary>
	/// Keeps a registry of credentials available.
	/// </summary>
	public class ZeroCredentialPool
	{
		private readonly ConcurrentQueue<Credential> _zeroValueCredentials = new ();

		internal ZeroCredentialPool()
		{
		}

		/// <summary>
		/// Registers those zero value credentials that were issued and return the valuable credentials.
		/// </summary>
		/// <param name="credentials">Credentials received from the coordinator.</param>
		public IEnumerable<Credential> RegisterZeroValueCredentials(IEnumerable<Credential> credentials)
		{
			foreach (var credential in credentials)
			{
				if (credential.Amount.IsZero)
				{
					_zeroValueCredentials.Enqueue(credential);
				}
				else
				{
					yield return credential;
				}
			}
		}

		public Credential[] CompleteWithZeroCredentials(IEnumerable<Credential> credentials)
		{
			var n = ProtocolConstants.CredentialNumber;
			var credentialsToReturn = new Credential[ProtocolConstants.CredentialNumber];
			foreach (var credential in credentials.Take(ProtocolConstants.CredentialNumber).ToArray())
			{
				n--;
				credentialsToReturn[n] = credential;
			}
			for (; n > 0; )
			{
				if (_zeroValueCredentials.TryDequeue(out var credential))
				{
					n--;
					credentialsToReturn[n] = credential;
				}
			}
			return credentialsToReturn;
		}
	}
}

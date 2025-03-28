using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  internal sealed class ShoppingCart : IShoppingCart
  {
    private readonly IDictionary<string, string> m_propertyBag = (IDictionary<string, string>) new Dictionary<string, string>();

    public ShoppingCart(Guid shoppingSessionId)
    {
      this.CreatedOn = DateTime.Now;
      this.ShoppingSessionId = shoppingSessionId;
    }

    public void SetType(ShoppingCartType type)
    {
      LogHelper.Instance.Log("SetType on Shopping Cart for Shopping Session '{0}': type={1}", (object) this.ShoppingSessionId, (object) type);
      this.Type = type;
    }

    public void SetEmail(string email)
    {
      LogHelper.Instance.Log("SetEmail on Shopping Cart for Shopping Session '{0}'", (object) this.ShoppingSessionId);
      this.Email = email;
    }

    public void SetPostalCode(string postalCode)
    {
      this.PostalCode = postalCode;
      LogHelper.Instance.Log("Set Postal Code on Shopping Cart for Shopping Session '{0}': postalCode={1}", (object) this.ShoppingSessionId, (object) postalCode);
    }

    public void SetReferenceNumber(long referenceNumber)
    {
      this.ReferenceNumber = new long?(referenceNumber);
      LogHelper.Instance.Log("Set Reference Number on Shopping Cart for Shopping Session '{0}': referenceNumber={1}", (object) this.ShoppingSessionId, (object) referenceNumber);
    }

    public void SetVariableAuthValues(
      bool authAcceptFlag,
      int authRuleId,
      Decimal authAmount,
      bool skipAuthRule)
    {
      LogHelper.Instance.Log("SetVariableAuthValues on Shopping Cart for Shopping Session '{0}': authAcceptFlag={1}, authRuleId={2}, authAmount={3}, skipAuthRule={4}", (object) this.ShoppingSessionId, (object) authAcceptFlag, (object) authRuleId, (object) authAmount, (object) skipAuthRule);
      this.AuthAcceptFlag = authAcceptFlag;
      this.AuthRuleId = authRuleId;
      this.AuthAmount = authAmount;
      this.SkipAuthRule = skipAuthRule;
    }

    public void SetPlayPassPromptAccepted(bool playPassPromptAccepted, int playPassPointsEarned)
    {
      LogHelper.Instance.Log("SetPlayPassPromptAccepted on Shopping Cart for Shopping Session '{0}': playPassPromptAccepted={1}, playPassPointsEarned={2}", (object) this.ShoppingSessionId, (object) playPassPromptAccepted, (object) playPassPointsEarned);
      this.PlayPassPromptAccepted = playPassPromptAccepted;
      this.PlayPassPointsEarned = playPassPointsEarned;
    }

    public string Email { get; internal set; }

    public string PostalCode { get; internal set; }

    public DateTime CreatedOn { get; internal set; }

    public long? ReferenceNumber { get; internal set; }

    public CreditElection UseCredits => CreditElection.OptedOut;

    public ShoppingCartType Type { get; internal set; }

    public Guid ShoppingSessionId { get; private set; }

    public bool AuthAcceptFlag { get; internal set; }

    public int AuthRuleId { get; internal set; }

    public Decimal AuthAmount { get; internal set; }

    public bool SkipAuthRule { get; internal set; }

    public bool PlayPassPromptAccepted { get; internal set; }

    public int PlayPassPointsEarned { get; internal set; }

    public void SetPropertyBagEntry(string key, string value)
    {
      if (string.IsNullOrWhiteSpace(key))
        return;
      this.m_propertyBag[key] = value;
    }

    public string GetPropertyBagEntry(string key)
    {
      return !this.m_propertyBag.ContainsKey(key) ? (string) null : this.m_propertyBag[key];
    }

    public void RemovePropertyBagEntry(string key)
    {
      if (!this.m_propertyBag.ContainsKey(key))
        return;
      this.m_propertyBag.Remove(key);
    }

    public IDictionary<string, string> PropertyBag => this.m_propertyBag;
  }
}

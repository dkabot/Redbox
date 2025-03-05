using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IShoppingCart
  {
    void SetType(ShoppingCartType type);

    void SetEmail(string email);

    void SetPostalCode(string postalCode);

    void SetReferenceNumber(long referenceNumber);

    void SetVariableAuthValues(
      bool authAcceptFlag,
      int authRuleId,
      Decimal authAmount,
      bool skipAuthRule);

    void SetPlayPassPromptAccepted(bool playPassPromptAccepted, int playPassPointsEarned);

    string Email { get; }

    string PostalCode { get; }

    DateTime CreatedOn { get; }

    long? ReferenceNumber { get; }

    CreditElection UseCredits { get; }

    ShoppingCartType Type { get; }

    bool AuthAcceptFlag { get; }

    int AuthRuleId { get; }

    Decimal AuthAmount { get; }

    bool SkipAuthRule { get; }

    bool PlayPassPromptAccepted { get; }

    int PlayPassPointsEarned { get; }

    string GetPropertyBagEntry(string key);

    void SetPropertyBagEntry(string key, string value);

    void RemovePropertyBagEntry(string key);

    IDictionary<string, string> PropertyBag { get; }
  }
}

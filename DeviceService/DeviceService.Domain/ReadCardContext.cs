using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;

namespace DeviceService.Domain
{
    public class ReadCardContext
    {
        private const int DEFAULT_TIMEOUT = 30000;
        private readonly ILogger<IUC285Proxy> _logger;
        private bool CompleteCallbackSent;

        public ReadCardContext(ILogger<IUC285Proxy> logger)
        {
            _logger = logger;
            Watch = new Stopwatch();
            Watch.Start();
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public ICardReadRequest Request { get; set; }

        public int? UserInteractionTimeout { get; set; }

        public int TimeRemainingUntilTimeout
        {
            get
            {
                var nullable = Request?.Timeout;
                if (_sentCardProcessing)
                    nullable = UserInteractionTimeout;
                var num = (long)(nullable ?? 30000);
                var watch = Watch;
                var elapsedMilliseconds = watch != null ? watch.ElapsedMilliseconds : 0L;
                return Math.Max(0, (int)(num - elapsedMilliseconds));
            }
        }

        public CancellationToken CancelToken { get; set; }

        public Action<Base87CardReadModel> JobCompletedCallback { get; set; }

        public Action<string, string> EventsCallback { get; set; }

        public CardSourceType CardSource { get; set; }

        public Stopwatch Watch { get; set; }

        public IList<Error> Errors { get; set; } = new List<Error>();

        public ReadCardContextState State { get; set; } = ReadCardContextState.Continue;

        public Base87CardReadModel ResponseDataModel { get; set; }

        public bool CardRemoved { get; set; }

        public CardBrandEnum CardBrand { get; set; }

        public CardStats CardStats { get; set; }

        public UnitDataModel UnitData { get; set; }

        public CardReadExitType ExitType { get; set; } = CardReadExitType.NotPerformed;

        public bool IsVasEnabled => Request.VasMode == VasMode.VasOnly || Request.VasMode == VasMode.VasAndPay;

        public bool IsPayEnabled => Request.VasMode == VasMode.PayOnly || Request.VasMode == VasMode.VasAndPay;

        public string VasData { get; set; }

        public WalletType Wallet { get; set; }

        private bool _sentCardProcessing { get; set; }

        public bool CanContinue(VasMode mode)
        {
            var flag = false;
            switch (mode)
            {
                case VasMode.PayOnly:
                    flag = IsPayEnabled && State.HasFlag(ReadCardContextState.PayContinue);
                    break;
                case VasMode.VasOnly:
                    flag = IsVasEnabled && State.HasFlag(ReadCardContextState.VasContinue);
                    break;
                case VasMode.VasAndPay:
                    flag = State == ReadCardContextState.Continue ||
                           (IsVasEnabled && State == ReadCardContextState.VasContinue) ||
                           (IsPayEnabled && State == ReadCardContextState.PayContinue);
                    break;
            }

            return flag;
        }

        public void UpdateStopStateFor(VasMode mode)
        {
            var state = State;
            switch (mode)
            {
                case VasMode.PayOnly:
                    if (State == ReadCardContextState.PayContinue)
                    {
                        State = ReadCardContextState.Stop;
                        break;
                    }

                    if (State == ReadCardContextState.Continue) State = ReadCardContextState.VasContinue;
                    break;
                case VasMode.VasOnly:
                    if (State == ReadCardContextState.VasContinue)
                    {
                        State = ReadCardContextState.Stop;
                        break;
                    }

                    if (State == ReadCardContextState.Continue) State = ReadCardContextState.PayContinue;
                    break;
                case VasMode.VasAndPay:
                    if (State.HasFlag(ReadCardContextState.Continue)) State = ReadCardContextState.Stop;
                    break;
            }

            var logger = _logger;
            if (logger == null)
                return;
            logger.LogInformation(string.Format("Update Stop State for VasMode: {0} - State Change From: {1} To: {2}",
                mode, state, State));
        }

        public void SendCompletedCallback()
        {
            if (CompleteCallbackSent)
                return;
            CompleteCallbackSent = true;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(string.Format("Send Card Read Model - Callback Exists: {0}, Model Exists: {1}",
                    JobCompletedCallback != null, ResponseDataModel != null));
            Watch.Stop();
            if (ResponseDataModel == null || CardBrandAndSourceIsDeactivated())
            {
                switch (CardSource)
                {
                    case CardSourceType.EMVContact:
                    case CardSourceType.QuickChip:
                        ResponseDataModel = new EMVCardReadModel();
                        break;
                    case CardSourceType.EMVContactless:
                    case CardSourceType.Mobile:
                        ResponseDataModel = new EMVCardReadModel();
                        break;
                    case CardSourceType.VASOnly:
                        ResponseDataModel = new EMVCardReadModel();
                        break;
                    default:
                        ResponseDataModel = new EncryptedCardReadModel();
                        break;
                }

                switch (State)
                {
                    case ReadCardContextState.Timeout:
                        var errors = Errors;
                        var error = new Error();
                        error.Code = "TIMEOUT";
                        var request = Request;
                        error.Message = string.Format("Command Timed Out.  Exceeded {0} Timeout",
                            request != null ? request.Timeout : 30000);
                        errors.Add(error);
                        ResponseDataModel.Status = ResponseStatus.Timeout;
                        break;
                    case ReadCardContextState.Canceled:
                        Errors.Add(new Error
                        {
                            Code = "CANCELLED",
                            Message = "Command Was Cancelled."
                        });
                        ResponseDataModel.Status = ResponseStatus.Cancelled;
                        break;
                    case ReadCardContextState.Tampered:
                        Errors.Add(new Error
                        {
                            Code = "TAMPERED",
                            Message = "Device is reporting Tampered Alert."
                        });
                        ResponseDataModel.Status = ResponseStatus.Tampered;
                        break;
                    case ReadCardContextState.Blocked:
                        Errors.Add(new Error
                        {
                            Code = "BLOCKED",
                            Message = string.Format("Card brand {0} and it's source {1} is disabled in system.config",
                                CardBrand, CardSource)
                        });
                        ResponseDataModel.Status = ResponseStatus.Blocked;
                        break;
                    default:
                        if (!IsVasEnabled || VasData == null)
                        {
                            ResponseDataModel.Status = ResponseStatus.Errored;
                            Errors.Add(new Error
                            {
                                Code = "SCC001",
                                Message = "Sending Default Error, ResponseDataModel Missing."
                            });
                        }

                        break;
                }
            }

            if (ResponseDataModel is EMVCardReadModel)
                ((EMVCardReadModel)ResponseDataModel).CardRemoved = CardRemoved;
            if (IsPayEnabled &&
                (CardSource == CardSourceType.EMVContactless || CardSource == CardSourceType.MSDContactless) &&
                Wallet != WalletType.None)
                CardSource = CardSourceType.Mobile;
            ResponseDataModel.TimeTaken = Watch.Elapsed;
            ResponseDataModel.AddErrors(Errors);
            ResponseDataModel.CardSource = CardSource;
            ResponseDataModel.VasData = VasData;
            ResponseDataModel.WalletFormat = Wallet;
            if (ResponseDataModel.Status == ResponseStatus.Success && ResponseDataModel.Errors.Any())
                ResponseDataModel.Status = ResponseStatus.Errored;
            CardStats = new CardStats
            {
                SessionId = Request?.SessionId,
                Id = Guid.NewGuid(),
                CardBrand = CardBrand,
                ManufacturerSerialNumber = UnitData?.ManufacturingSerialNumber,
                RBAVersion = UnitData?.GetApplicationVersionString(),
                ReadResult = ExitType,
                SourceType = CardSource,
                ErrorCode = ResponseDataModel.ErrorCode,
                WalletFormat = Wallet,
                HasPayData = ResponseDataModel.HasPayData,
                HasVasData = ResponseDataModel.HasVasData,
                VasErrorCode = ResponseDataModel.VasErrorCode
            };
            var completedCallback = JobCompletedCallback;
            if (completedCallback == null)
                return;
            completedCallback(ResponseDataModel);
        }

        public void SendCardRemoved()
        {
            CardRemoved = true;
            var eventsCallback = EventsCallback;
            if (eventsCallback == null)
                return;
            eventsCallback("CardRemovedResponseEvent", null);
        }

        public void SendCardProcessing()
        {
            if (_sentCardProcessing)
                return;
            var eventsCallback = EventsCallback;
            if (eventsCallback != null)
                eventsCallback("CardProcessingStartedResponseEvent", CardSource.ToString());
            _sentCardProcessing = true;
            Watch.Restart();
            _logger.LogInformation(string.Format("Set New Timeout: {0}", UserInteractionTimeout));
        }

        public bool CardBrandAndSourceIsDeactivated()
        {
            if (Request.ExcludeCardBrandBySource == null)
                return false;
            var flag = false;
            var nullable = new CardBrandAndSource?();
            foreach (var brandAndSource in Request.ExcludeCardBrandBySource)
            {
                var generalSourceType1 = brandAndSource.GetGeneralSourceType();
                var generalSourceType2 = CardSource.GetGeneralSourceType();
                if (generalSourceType1 == generalSourceType2 && generalSourceType1 != GeneralSourceType.None)
                {
                    var brand = brandAndSource.GetBrand();
                    var cardBrand = CardBrand;
                    if ((brand.GetValueOrDefault() == cardBrand) & brand.HasValue)
                    {
                        flag = true;
                        nullable = brandAndSource;
                        break;
                    }
                }
            }

            if (flag)
            {
                State = ReadCardContextState.Blocked;
                var logger = _logger;
                if (logger != null)
                    logger.LogInformation(string.Format("{0} is deactivated in system.config", nullable));
            }

            return flag;
        }
    }
}
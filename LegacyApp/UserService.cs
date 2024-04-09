using System;

namespace LegacyApp
{
    public interface ICreditLimitService
    {
        int GetCreditLimit(string lastName, DateTime birthdate);
    }
    
    public interface IClientRepository
    {
        Client GetById(int idClient);
    }
    
    public class UserService
    {
        private readonly IClientRepository _clientRepository;
        private readonly ICreditLimitService _creditService;

        [Obsolete]
        public UserService()
        {
            _clientRepository = new ClientRepository();
            _creditService = new UserCreditService();
        }
        
        public UserService(IClientRepository clientRepository, ICreditLimitService creditService)
        {
            _clientRepository = clientRepository;
            _creditService = creditService;
        }
        
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            var client = _clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            if (!user.IsValidData() || !user.IsEligible())
            {
                return false;
            }
            
            user.HasCreditLimit = true;
            int creditLimitMultiplier = 1;

            switch (client.Type)
            {
                case ClientType.VERY_IMPORTANT:
                    user.HasCreditLimit = false;
                    break;
                case ClientType.IMPORTANT:
                    creditLimitMultiplier = 2;
                    break;
            }

            if (user.HasCreditLimit)
            {
                int creditLimit = _creditService.GetCreditLimit(user.LastName, user.DateOfBirth) * creditLimitMultiplier;
                user.CreditLimit = creditLimit;
                
                if (creditLimit < 500)
                {
                    return false;
                }
            }

            UserDataAccess.AddUser(user);
            return true;
        }
    }
}

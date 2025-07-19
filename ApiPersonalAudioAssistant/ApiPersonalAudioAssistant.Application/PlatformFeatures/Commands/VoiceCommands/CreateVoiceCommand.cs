using MediatR;
using ApiPersonalAudioAssistant.Application.Interfaces;
using ApiPersonalAudioAssistant.Domain.Entities;

namespace ApiPersonalAudioAssistant.Application.PlatformFeatures.Commands.VoiceCommands
{
   public class CreateVoiceCommand : IRequest<CreatedVoice>
    {
        public string VoiceId { get; set; }
        public string Name { get; set; }
        public string? UserId { get; set; }
   }

    public class CreateVoiceCommandHandler : IRequestHandler<CreateVoiceCommand, CreatedVoice>
    {
        private readonly IVoiceRepository _voiceRepository;

        public CreateVoiceCommandHandler(IVoiceRepository voiceRepository)
        {
            _voiceRepository = voiceRepository;
        }

        public async Task<CreatedVoice> Handle(CreateVoiceCommand request, CancellationToken cancellationToken = default)
        {
            /*
            var user = await _voiceRepository.GetVoiceByIdAsync(request.UserId, cancellationToken);
            if (user != null)
            {
                throw new Exception("Voice already exists.");
            }*/

            var newVoice = new Voice()
            {
                VoiceId = request.VoiceId,
                Name = request.Name,
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);

            CreatedVoice createdVoice = new()
            {
                VoiceId = newVoice.Id.ToString()
            };

            return createdVoice;


            /*
            
            var user = await _voiceRepository.GetVoiceByIdAsync(request.UserId, cancellationToken);
            if (user != null)
            {
                throw new Exception("Voice already exists.");
            }
            
            var newVoice = new Voice()
            {
                VoiceId = request.VoiceId,
                Name = request.Name,
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);

            return newVoice.Id.ToString();
            var newVoice = new Voice()
            {
                VoiceId = "knrPHWnBmmDHMoiMeP3l",
                Name = "🎅 Santa Claus",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/knrPHWnBmmDHMoiMeP3l/6b454051-3ec2-4a6d-aa2f-1bda2d8c93ea.mp3",
                Gender = "Чоловік",
                Age = "Старшого віку",
                UseCase = "Персонажі",
                Description = "Веселий",
                UserId = null
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);



            newVoice = new Voice()
            {
                VoiceId = "GBv7mTt0atIp3Br8iCZE",
                Name = "Thomas",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/GBv7mTt0atIp3Br8iCZE/98542988-5267-4148-9a9e-baa8c4f14644.mp3",
                Gender = "Чоловік",
                Age = "Молодий",
                UseCase = "Медитація",
                Description = "Спокійний",
                UserId = null
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);
            ////////////////
            /// 

            newVoice = new Voice()
            {
                VoiceId = "pMsXgVXv3BLzUgSXRplE",
                Name = "Serena",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/pMsXgVXv3BLzUgSXRplE/d61f18ed-e5b0-4d0b-a33c-5c6e7e33b053.mp3",
                Gender = "Жінка",
                Age = "Середнього віку",
                UseCase = "Розповідь",
                Description = "Приємний",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);

            ///////////////
            ////
            ///

            newVoice = new Voice()
            {
                VoiceId = "5Q0t7uMcjvnagumLfvZi",
                Name = "Paul",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/5Q0t7uMcjvnagumLfvZi/a4aaa30e-54c4-44a4-8e46-b9b00505d963.mp3",
                Gender = "Чоловік",
                Age = "Середнього віку",
                UseCase = "Новини",
                Description = "Авторитетний",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);
            ///////////////
            ///

            newVoice = new Voice()
            {
                VoiceId = "ODq5zmih8GrVes37Dizd",
                Name = "Patrick",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/ODq5zmih8GrVes37Dizd/0ebec87a-2569-4976-9ea5-0170854411a9.mp3",
                Gender = "Чоловік",
                Age = "Середнього віку",
                UseCase = "Персонажі",
                Description = "Гучний",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);


            /////////////////
            ///

            newVoice = new Voice()
            {
                VoiceId = "pFZP5JQG7iQjIQuC4Bku",
                Name = "Lily",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/pFZP5JQG7iQjIQuC4Bku/89b68b35-b3dd-4348-a84a-a3c13a3c2b30.mp3",
                Gender = "Жіночий",
                Age = "Середнього віку",
                UseCase = "Розповідь",
                Description = "Теплий",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);

            ////////////
            ///

            newVoice = new Voice()
            {
                VoiceId = "21m00Tcm4TlvDq8ikWAM",
                Name = "Rachel",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/21m00Tcm4TlvDq8ikWAM/b4928a68-c03b-411f-8533-3d5c299fd451.mp3",
                Gender = "Жіночий",
                Age = "Молодий",
                UseCase = "Розповідь",
                Description = "Спокійний",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);


            /////////////////
            ///

            newVoice = new Voice()
            {
                VoiceId = "FGY2WhTYpPnrIDTdsKH5",
                Name = "Laura",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/FGY2WhTYpPnrIDTdsKH5/67341759-ad08-41a5-be6e-de12fe448618.mp3",
                Gender = "Жіночий",
                Age = "Молодий",
                UseCase = "Розповідь",
                Description = "Енергійний",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);



            /////////////////
            ///

            newVoice = new Voice()
            {
                VoiceId = "TxGEqnHWrfWFTfGW9XjX",
                Name = "Josh",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/TxGEqnHWrfWFTfGW9XjX/47de9a7e-773a-42a8-b410-4aa90c581216.mp3",
                Gender = "Чоловічий",
                Age = "Молодий",
                UseCase = "Нарація",
                Description = "Глибокий",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);

            /////////////////
            ///

            newVoice = new Voice()
            {
                VoiceId = "ZQe5CZNOzWyzPSCn5a3c",
                Name = "James",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/ZQe5CZNOzWyzPSCn5a3c/35734112-7b72-48df-bc2f-64d5ab2f791b.mp3",
                Gender = "Чоловічий",
                Age = "Старший",
                UseCase = "Новини",
                Description = "Спокійний",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);

            /////////////////
            ///

            newVoice = new Voice()
            {
                VoiceId = "SOYHLrjzK2X1ezoPC6cr",
                Name = "Harry",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/SOYHLrjzK2X1ezoPC6cr/86d178f6-f4b6-4e0e-85be-3de19f490794.mp3",
                Gender = "Чоловічий",
                Age = "Молодий",
                UseCase = "Розповідь",
                Description = "Тривожний",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);


            /////////////////
            ///


            newVoice = new Voice()
            {
                VoiceId = "Xb7hH8MSUJpSbSDYk0k2",
                Name = "Alice",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/Xb7hH8MSUJpSbSDYk0k2/d10f7534-11f6-41fe-a012-2de1e482d336.mp3",
                Gender = "Жіночий",
                Age = "Середнього віку",
                UseCase = "Новини",
                Description = "Самовпевнений",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);



            /////////////////
            /// 

            newVoice = new Voice()
            {
                VoiceId = "pNInz6obpgDQGcFmaJgB",
                Name = "Adam",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/pNInz6obpgDQGcFmaJgB/d6905d7a-dd26-4187-bfff-1bd3a5ea7cac.mp3",
                Gender = "Чоловічий",
                Age = "Середнього віку",
                UseCase = "Наррація",
                Description = "Впевнений",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);

            /////////////////
            /// 

            newVoice = new Voice()
            {
                VoiceId = "LcfcDJNUP1GQjkzn1xUU",
                Name = "Emily",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/LcfcDJNUP1GQjkzn1xUU/e4b994b7-9713-4238-84f3-add8fccaaccd.mp3",
                Gender = "Жіночий",
                Age = "Молодий",
                UseCase = "Медитація",
                Description = "Спокійний",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);


            /////////////////
            /// 

            newVoice = new Voice()
            {
                VoiceId = "g5CIjZEefAph4nQFvHAz",
                Name = "Ethan",
                URL = "https://storage.googleapis.com/eleven-public-prod/premade/voices/g5CIjZEefAph4nQFvHAz/26acfa99-fdec-43b8-b2ee-e49e75a3ac16.mp3",
                Gender = "Чоловічий",
                Age = "Молодий",
                UseCase = "ASMR",
                Description = "М’який",
                UserId = request.UserId
            };
            await _voiceRepository.CreateVoice(newVoice, cancellationToken);
            */
        }
    }

    public class CreatedVoice
    {
        public string VoiceId { get; set; }
    }
}

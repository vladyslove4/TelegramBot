using AutoMapper;
using Telegram.Bot.Types;
using TelegramBot.Domain.Model;

namespace TelegramBot.DAL;

internal class MappingProfile: Profile
{
    public MappingProfile()
    {
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Chat.FirstName));
    }    
}
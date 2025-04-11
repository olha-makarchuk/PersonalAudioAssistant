using CommunityToolkit.Mvvm.ComponentModel;
using Java.IO;
using PersonalAudioAssistant.Domain.Entities;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using static Android.Util.EventLogTags;

public partial class VoiceFilterModel : ObservableObject
{
    [ObservableProperty]
    private string filterDescription;

    [ObservableProperty]
    private string filterAge;

    [ObservableProperty]
    private string filterGender;

    [ObservableProperty]
    private string filterUseCase;


    public ObservableCollection<Voice> ApplyFilter(List<Voice> allVoices)
    {
        var filtered = allVoices.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(FilterDescription))
            filtered = filtered.Where(v => v.Description != null &&
                v.Description.Contains(FilterDescription, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterAge))
            filtered = filtered.Where(v => v.Age != null &&
                v.Age.Contains(FilterAge, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterGender))
            filtered = filtered.Where(v => v.Gender != null &&
                v.Gender.Contains(FilterGender, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterUseCase))
            filtered = filtered.Where(v => v.UseCase != null &&
                v.UseCase.Contains(FilterUseCase, StringComparison.OrdinalIgnoreCase));

        return new ObservableCollection<Voice>(filtered);
    }

    public void ResetDescriptionFilter() => FilterDescription = null;
    public void ResetAgeFilter() => FilterAge = null;
    public void ResetGenderFilter() => FilterGender = null;
    public void ResetUseCaseFilter() => FilterUseCase = null;
}
using CommunityToolkit.Mvvm.ComponentModel;
using Java.IO;
using PersonalAudioAssistant.Application.Services.Api;
using System.Collections.ObjectModel;
using System.Xml.Linq;

public partial class VoiceFilterModel : ObservableObject
{
    // Параметри фільтрації
    [ObservableProperty]
    private string filterAccent;

    [ObservableProperty]
    private string filterDescription;

    [ObservableProperty]
    private string filterAge;

    [ObservableProperty]
    private string filterGender;

    [ObservableProperty]
    private string filterUseCase;


    public ObservableCollection<Voice> ApplyFilter(System.Collections.Generic.List<Voice> allVoices)
    {
        var filtered = allVoices.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(FilterAccent))
            filtered = filtered.Where(v => v.labels?.accent != null &&
                v.labels.accent.Contains(FilterAccent, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterDescription))
            filtered = filtered.Where(v => v.labels?.description != null &&
                v.labels.description.Contains(FilterDescription, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterAge))
            filtered = filtered.Where(v => v.labels?.age != null &&
                v.labels.age.Contains(FilterAge, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterGender))
            filtered = filtered.Where(v => v.labels?.gender != null &&
                v.labels.gender.Contains(FilterGender, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FilterUseCase))
            filtered = filtered.Where(v => v.labels?.use_case != null &&
                v.labels.use_case.Contains(FilterUseCase, StringComparison.OrdinalIgnoreCase));

        return new ObservableCollection<Voice>(filtered);
    }

    public void ResetAccentFilter() => FilterAccent = null;
    public void ResetDescriptionFilter() => FilterDescription = null;
    public void ResetAgeFilter() => FilterAge = null;
    public void ResetGenderFilter() => FilterGender = null;
    public void ResetUseCaseFilter() => FilterUseCase = null;
}
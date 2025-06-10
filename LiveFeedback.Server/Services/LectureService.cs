using System.Collections.Concurrent;
using LiveFeedback.BlazorFrontend;
using LiveFeedback.Shared.Enums;
using Client = LiveFeedback.Shared.Models.Client;
using Lecture = LiveFeedback.Shared.Models.Lecture;

namespace LiveFeedback.Server.Services;

public abstract class LectureService
{
    private static readonly ConcurrentDictionary<string, Lecture> Lectures = new();
    private static readonly Shared.GlobalConfig GlobalConfig = new();

    public static string StartNewLecture(Client initialPresenter)
    {
        Lecture lecture = new()
        {
            Id = Guid.NewGuid().ToString(),
        };

        lecture.ConnectedPresenters.TryAdd(initialPresenter.Id, initialPresenter);
        Lectures.TryAdd(lecture.Id, lecture);
        return lecture.Id;
    }

    public static List<Lecture> GetLecturesUserIsConnectedTo(string userId)
    {
        List<Lecture> lecturesFound = [];
        lecturesFound.AddRange(Lectures.Values.Where(lecture =>
            lecture.ConnectedClients.Any(c => c.Value.Id == userId) ||
            lecture.ConnectedPresenters.Any(p => p.Value.Id == userId)));
        return lecturesFound;
    }

    public static void RemoveClientFromPotentialLecturesByClientId(string clientId)
    {
        List<Lecture> lecturesFound = GetLecturesUserIsConnectedTo(clientId);
        foreach (Lecture lecture in lecturesFound)
        {
            Lectures[lecture.Id].ConnectedClients.TryRemove(clientId, out _);
            Lectures[lecture.Id].ConnectedPresenters.TryRemove(clientId, out _);
        }
    }

    public static void RemoveClientFromPotentialLecturesByConnectionId(string connectionId)
    {
        List<Lecture> lecturesFound =
            Lectures.Where(l =>
                    l.Value.ConnectedClients.Any(c => c.Value.ConnectionId == connectionId) ||
                    l.Value.ConnectedPresenters.Any(p => p.Value.ConnectionId == connectionId))
                .Select(kvp => kvp.Value).ToList();

        foreach (Lecture lecture in lecturesFound)
        {
            Lectures[lecture.Id].ConnectedClients.TryRemove(connectionId, out _);
            Lectures[lecture.Id].ConnectedPresenters.TryRemove(connectionId, out _);
        }
    }

    public static void UpdateRating(string clientId, ushort newRating)
    {
        List<Lecture> lecturesFound = GetLecturesUserIsConnectedTo(clientId);
        foreach (Lecture lecture in lecturesFound)
        {
            if (!Lectures.TryGetValue(lecture.Id, out Lecture? _))
            {
                continue;
            }

            if (lecture.ConnectedClients.TryGetValue(clientId, out Client? client))
            {
                client.Rating = newRating;
            }
        }
    }

    public static string? GetLectureIdByClientId(string clientId)
    {
        Lecture[] lecturesFound = Lectures.Values.Where(l =>
            l.ConnectedClients.Values.Concat(l.ConnectedPresenters.Values).Any(c => c.Id == clientId)).ToArray();
        if (lecturesFound.Length == 0)
        {
            return null;
        }

        return lecturesFound[0].Id;
    }

    public static string GetSingleLectureIdIfSingleLecture()
    {
        if (Lectures.Values.Count != 1)
            return "";
        return Lectures.Values.First().Id;
    }

    public static void ResetLecture(string lectureId)
    {
        if (Lectures.TryGetValue(lectureId, out Lecture? lecture))
        {
            lecture.ConnectedClients.Clear();
            Console.WriteLine("Lecture was cleared successfully");
        }
        else
        {
            Console.WriteLine("Lecture couldn't be found");
        }
    }

    public static void DeleteLecture(string lectureId)
    {
        Lectures.TryRemove(lectureId, out _);
    }

    public static void AddClient(Client client, string lectureId)
    {
        if (!Lectures.TryGetValue(lectureId, out Lecture? lecture))
            return;

        if (!lecture.ConnectedClients.TryAdd(client.Id, client))
            Console.WriteLine("ERROR: Failed to add client to connected clients");
    }

    public static void AddPresenter(Client client, string lectureId)
    {
        if (!Lectures.TryGetValue(lectureId, out Lecture? lecture))
            return;

        if (!lecture.ConnectedPresenters.TryAdd(client.Id, client))
            Console.WriteLine("Trying to add presenter…");
    }

    public static void ReaddClient(Client client, string lectureId)
    {
        if (!Lectures.TryGetValue(lectureId, out Lecture? lecture))
            return;

        if (lecture.ConnectedClients.TryGetValue(client.Id, out Client? oldClient))
            lecture.ConnectedClients.TryUpdate(client.Id, client, oldClient);
    }

    public static void ReaddPresenter(Client client, string lectureId)
    {
        if (Lectures.TryGetValue(lectureId, out Lecture? lecture))
        {
            lecture.ConnectedPresenters.TryUpdate(client.Id, client, lecture.ConnectedPresenters[client.Id]);
        }
    }

    public static Lecture? GetLecture(string lectureId)
    {
        Lectures.TryGetValue(lectureId, out Lecture? lecture);
        return lecture;
    }

    public static string[] GetPresenterConnectionIds(string lectureId)
    {
        return Lectures
            .Where(l => l.Key == lectureId)
            .SelectMany(l => l.Value.ConnectedPresenters
                .Select(p => p.Value.ConnectionId)
            ).ToArray();
    }

    public static List<Lecture> GetCurrentLectures()
    {
        return Lectures.Values.ToList();
    }
}
using System.Collections.Concurrent;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Models;
using Lecture = LiveFeedback.Server.Models.Lecture;

namespace LiveFeedback.Server.Services;

public class LectureService
{
    private static readonly ConcurrentDictionary<string, Lecture> _lectures = new();
    private static readonly Shared.GlobalConfig _globalConfig = new();

    public static string StartNewLecture(Client initialPresenter)
    {
        Lecture lecture = new()
        {
            Id = Guid.NewGuid().ToString(),
        };

        lecture.ConnectedPresenters.TryAdd(initialPresenter.Id, initialPresenter);
        _lectures.TryAdd(lecture.Id, lecture);
        return lecture.Id;
    }

    public static List<Lecture> GetLecturesUserIsConnectedTo(string userId)
    {
        List<Lecture> lecturesFound = [];
        lecturesFound.AddRange(_lectures.Values.Where(lecture =>
            lecture.ConnectedClients.Any(c => c.Value.Id == userId) ||
            lecture.ConnectedPresenters.Any(p => p.Value.Id == userId)));
        return lecturesFound;
    }

    public static void RemoveClientFromPotentialLecturesByClientID(string clientId)
    {
        List<Lecture> lecturesFound = GetLecturesUserIsConnectedTo(clientId);
        foreach (Lecture lecture in lecturesFound)
        {
            _lectures[lecture.Id].ConnectedClients.TryRemove(clientId, out _);
            _lectures[lecture.Id].ConnectedPresenters.TryRemove(clientId, out _);
        }
    }

    public static void RemoveClientFromPotentialLecturesByConnectionID(string connectionId)
    {
        List<Lecture> lecturesFound =
            _lectures.Where(l =>
                    l.Value.ConnectedClients.Any(c => c.Value.ConnectionId == connectionId) ||
                    l.Value.ConnectedPresenters.Any(p => p.Value.ConnectionId == connectionId))
                .Select(kvp => kvp.Value).ToList();

        foreach (Lecture lecture in lecturesFound)
        {
            _lectures[lecture.Id].ConnectedClients.TryRemove(connectionId, out _);
            _lectures[lecture.Id].ConnectedPresenters.TryRemove(connectionId, out _);
        }
    }

    public static void UpdateRating(string clientId, ushort newRating)
    {
        List<Lecture> lecturesFound = GetLecturesUserIsConnectedTo(clientId);
        foreach (Lecture lecture in lecturesFound)
        {
            if (!_lectures.TryGetValue(lecture.Id, out Lecture? value))
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
        Lecture[] lecturesFound = _lectures.Values.Where(l =>
            l.ConnectedClients.Values.Concat(l.ConnectedPresenters.Values).Any(c => c.Id == clientId)).ToArray();
        if (lecturesFound.Length == 0)
        {
            return null;
        }

        return lecturesFound[0].Id;
    }

    public static string GetSingleLectureIdInLocalMode()
    {
        if (_globalConfig.Mode != Mode.Local)
        {
            throw new Exception(
                "Mode not local but expected. GetSingleLectureIdInLocalMode was called when running in any mode but local. Make sure environment variables are set correctly.");
        }

        return _lectures.Values.First().Id;
    }

    public static void ResetLecture(string lectureId)
    {
        if (_lectures.TryGetValue(lectureId, out Lecture? lecture))
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
        _lectures.TryRemove(lectureId, out _);
    }

    public static void AddClient(Client client, string lectureId)
    {
        if (_lectures.TryGetValue(lectureId, out Lecture? lecture))
        {
            lecture.ConnectedClients.TryAdd(client.Id, client);
            Console.WriteLine("Trying to add client…");
        }
    }

    public static void AddPresenter(Client client, string lectureId)
    {
        if (_lectures.TryGetValue(lectureId, out Lecture? lecture))
        {
            lecture.ConnectedPresenters.TryAdd(client.Id, client);
            Console.WriteLine("Trying to add presenter…");
        }
    }

    public static void ReaddClient(Client client, string lectureId)
    {
        if (_lectures.TryGetValue(lectureId, out Lecture? lecture))
        {
            if (lecture.ConnectedClients.TryGetValue(client.Id, out Client? oldClient))
                lecture.ConnectedClients.TryUpdate(client.Id, client, oldClient);
        }
    }

    public static void ReaddPresenter(Client client, string lectureId)
    {
        if (_lectures.TryGetValue(lectureId, out Lecture? lecture))
        {
            lecture.ConnectedPresenters.TryUpdate(client.Id, client, lecture.ConnectedPresenters[client.Id]);
        }
    }

    public static Lecture? GetLecture(string lectureId)
    {
        return _lectures[lectureId] ?? null;
    }

    public static string[] GetPresenterConnectionIds(string lectureId)
    {
        return _lectures
            .Where(l => l.Key == lectureId)
            .SelectMany(l => l.Value.ConnectedPresenters
                .Select(p => p.Value.ConnectionId)
            ).ToArray();
    }
}
using Brightcove.Core.Exceptions;
using Brightcove.Core.Extensions;
using Brightcove.Core.Models;
using Brightcove.MediaFramework.Brightcove.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Brightcove.Core.Services
{
    public class BrightcoveService
    {
        readonly static HttpClient client = BrightcoveHttpClient.Instance;

        int retryMax = 3;
        int retryAttempt = 0;

        readonly string cmsBaseUrl = "https://cms.api.brightcove.com/v1/accounts";
        readonly string ingestBaseUrl = "https://ingest.api.brightcove.com/v1/accounts";
        readonly string playersBaseUrl = "https://players.api.brightcove.com/v1/accounts";
        readonly string experienceBaseUrl = "https://experiences.api.brightcove.com/v1/accounts";
        readonly string accountId;
        readonly BrightcoveAuthenticationService authenticationService;

        public BrightcoveService(string accountId, string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                throw new ArgumentException("argument must not be null or empty", nameof(accountId));
            }

            this.accountId = accountId.Trim();

            authenticationService = new BrightcoveAuthenticationService(clientId, clientSecret);
        }

        public string IngestVideo(string videoId, string url)
        {
            IngestVideo video = new IngestVideo();
            video.IngestMaster = new IngestMaster();
            video.IngestMaster.Url = url;

            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(JsonConvert.SerializeObject(video), Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri($"{ingestBaseUrl}/{accountId}/videos/{videoId}/ingest-requests");

            HttpResponseMessage response = SendRequest(request);
            IngestJobId ingestJobId = JsonConvert.DeserializeObject<IngestJobId>(response.Content.ReadAsString());

            return ingestJobId.JobId;
        }

        public TemporaryIngestUrls GetTemporaryIngestUrls(string videoId)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{ingestBaseUrl}/{accountId}/videos/{videoId}/upload-urls/{videoId}");

            HttpResponseMessage response = SendRequest(request);
            TemporaryIngestUrls urls = JsonConvert.DeserializeObject<TemporaryIngestUrls>(response.Content.ReadAsString());

            return urls;
        }

        public Video CreateVideo(string name)
        {
            Video video = new Video();
            video.Name = name;

            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(JsonConvert.SerializeObject(video), Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos");

            HttpResponseMessage response = SendRequest(request);
            video = JsonConvert.DeserializeObject<Video>(response.Content.ReadAsString());

            return video;
        }

        public Folder CreateFolder(string name)
        {
            Folder folder = new Folder();
            folder.Name = name;

            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(JsonConvert.SerializeObject(folder), Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/folders");

            HttpResponseMessage response = SendRequest(request);
            folder = JsonConvert.DeserializeObject<Folder>(response.Content.ReadAsString());

            return folder;
        }

        public PlayList CreatePlaylist(string name)
        {
            PlayList playlist = new PlayList();
            playlist.Name = name;
            playlist.PlaylistType = "EXPLICIT";

            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(JsonConvert.SerializeObject(playlist), Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/playlists");

            HttpResponseMessage response = SendRequest(request);
            playlist = JsonConvert.DeserializeObject<PlayList>(response.Content.ReadAsString());

            return playlist;
        }

        public Label CreateLabel(string path)
        {
            Label label = new Label();
            label.Path = path;

            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(JsonConvert.SerializeObject(label), Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/labels");

            HttpResponseMessage response = SendRequest(request);
            label = new Label(JsonConvert.DeserializeObject<Label>(response.Content.ReadAsString()).Path);

            return label;
        }

        public Label UpdateLabel(Label label)
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = new HttpMethod("PATCH");
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/labels/by_path/{label.Path}");

            request.Content = new StringContent(JsonConvert.SerializeObject(label), Encoding.UTF8, "application/json");

            HttpResponseMessage response = SendRequest(request);

            return new Label(JsonConvert.DeserializeObject<Label>(response.Content.ReadAsString()).Path);
        }

        public void MoveToFolder(Video video, string folderId)
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Put;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/folders/{folderId}/videos/{video.Id}");

            SendRequest(request);

            return;
        }

        public void RemoveFromFolder(Video video, string folderId)
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Delete;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/folders/{folderId}/videos/{video.Id}");

            SendRequest(request);

            return;
        }

        public bool TryGetLabel(string path, out Label label)
        {
            //The Brightcove API does not actually include anyway to check if a specific label exists
            //So we have to do this...
            IEnumerable<Label> labels = GetLabels();
            label = labels.Where(l => l.Path == path).FirstOrDefault();

            if(label == null)
            {
                return false;
            }

            return true;
        }

        public VideoVariant CreateVideoVariant(string videoId, string videoVariantName, string language)
        {
            VideoVariant videoVariant = new VideoVariant();
            videoVariant.Language = language;
            videoVariant.Name = videoVariantName;

            if (videoId.Contains(","))
            {
                throw new ArgumentException("the video ID must not contain any commas", nameof(videoId));
            }

            if (string.IsNullOrWhiteSpace(videoVariantName))
            {
                throw new ArgumentException("the video name cannnot be null or empty string", nameof(videoVariantName));
            }

            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(JsonConvert.SerializeObject(videoVariant), Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos/{videoId}/variants");

            HttpResponseMessage response = SendRequest(request);
            videoVariant = JsonConvert.DeserializeObject<VideoVariant>(response.Content.ReadAsString());
            videoVariant.Id = videoId;

            return videoVariant;
        }

        public void DeletePlaylist(string playlistId)
        {
            if (string.IsNullOrWhiteSpace(playlistId))
            {
                return;
            }

            if (playlistId.Contains(","))
            {
                throw new ArgumentException("the playlist ID must not contain any commas", nameof(playlistId));
            }

            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Delete;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/playlists/{playlistId}");

            SendRequest(request);

            return;
        }

        public void DeleteVideo(string videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                return;
            }

            if (videoId.Contains(","))
            {
                throw new ArgumentException("the video ID must not contain any commas", nameof(videoId));
            }

            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Delete;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos/{videoId}");

            SendRequest(request);

            return;
        }

        public void DeleteFolder(string folderId)
        {
            if (string.IsNullOrWhiteSpace(folderId))
            {
                return;
            }

            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Delete;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/folders/{folderId}");

            SendRequest(request);

            return;
        }

        public void DeleteVideoVariant(string videoId, string language)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                return;
            }

            if (videoId.Contains(","))
            {
                throw new ArgumentException("the video ID must not contain any commas", nameof(videoId));
            }

            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Delete;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos/{videoId}/variants/{language}");

            SendRequest(request);

            return;
        }

        public Video UpdateVideo(Video video)
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = new HttpMethod("PATCH");
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos/{video.Id}");

            //Some properties will cause an invalid response because they are not updateable
            //Setting the property to null should remove it from the serialized request
            //Use shallowcopy to avoid side-effects caused by mutating the reference
            Video newVideo = video.ShallowCopy();
            newVideo.Id = null;
            newVideo.Images = null;
            newVideo.Variants = null;

            string content = JsonConvert.SerializeObject(newVideo);

            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            HttpResponseMessage response = SendRequest(request);

            return JsonConvert.DeserializeObject<Video>(response.Content.ReadAsString());
        }

        public Folder UpdateFolder(Folder folder)
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = new HttpMethod("PATCH");
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/folders/{folder.Id}");

            //We can only update the name field (rename) a folder so we ignore all other fields
            Folder newFolder = new Folder() { Name = folder.Name };
            
            request.Content = new StringContent(JsonConvert.SerializeObject(newFolder), Encoding.UTF8, "application/json");

            HttpResponseMessage response = SendRequest(request);

            return JsonConvert.DeserializeObject<Folder>(response.Content.ReadAsString());
        }

        public VideoVariant UpdateVideoVariant(VideoVariant videoVariant)
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = new HttpMethod("PATCH");
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos/{videoVariant.Id}/variants/{videoVariant.Language}");

            //Some properties will cause an invalid response because they are not updateable
            //Setting the property to null should remove it from the serialized request
            //Use shallowcopy to avoid side-effects caused by mutating the reference
            VideoVariant newVideoVariant = videoVariant.ShallowCopy();
            newVideoVariant.Language = null;

            request.Content = new StringContent(JsonConvert.SerializeObject(newVideoVariant), Encoding.UTF8, "application/json");
            HttpResponseMessage response = SendRequest(request);

            return JsonConvert.DeserializeObject<VideoVariant>(response.Content.ReadAsString());
        }


        public Player CreatePlayer(string name, string description)
        {
            var playerRequest = new { name = name, description = description };

            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent(JsonConvert.SerializeObject(playerRequest), Encoding.UTF8, "application/json");
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri($"{playersBaseUrl}/{accountId}/players");

            HttpResponseMessage response = SendRequest(request);
            return JsonConvert.DeserializeObject<Player>(response.Content.ReadAsString());
        }

        public Player UpdatePlayer(Player player)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = new HttpMethod("PATCH");
            request.RequestUri = new Uri($"{playersBaseUrl}/{accountId}/players/{player.Id}");

            var newPlayer = new { name = player.Name, description = player.ShortDescription };

            request.Content = new StringContent(JsonConvert.SerializeObject(newPlayer), Encoding.UTF8, "application/json");
            HttpResponseMessage response = SendRequest(request);
            return JsonConvert.DeserializeObject<Player>(response.Content.ReadAsString());
        }

        public void DeletePlayer(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
                return;

            if (playerId.Contains(","))
                throw new ArgumentException("the player ID must not contain any commas", nameof(playerId));

            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Delete;
            request.RequestUri = new Uri($"{playersBaseUrl}/{accountId}/players/{playerId}");
            SendRequest(request);
        }

        public void DeleteLabel(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Delete;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/labels/by_path/{path}");
            SendRequest(request);
        }

        public PlayList UpdatePlaylist(PlayList playlist)
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = new HttpMethod("PATCH");
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/playlists/{playlist.Id}");

            //Some properties will cause an invalid response because they are not updateable
            //Setting the property to null should remove it from the serialized request
            //Use shallowcopy to avoid side-effects caused by mutating the reference
            PlayList newPlaylist = playlist.ShallowCopy();
            newPlaylist.Id = null;
            newPlaylist.Images = null;
            newPlaylist.CreationDate = null;
            newPlaylist.LastModifiedDate = null;

            request.Content = new StringContent(JsonConvert.SerializeObject(newPlaylist), Encoding.UTF8, "application/json");

            HttpResponseMessage response = SendRequest(request);

            return JsonConvert.DeserializeObject<PlayList>(response.Content.ReadAsString());
        }

        public IEnumerable<PlayList> GetPlayLists(int offset = 0, int limit = 20, string sort = "", string query = "")
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/playlists?offset={offset}&limit={limit}&sort={sort}&q={query}");

            HttpResponseMessage response = SendRequest(request);

            return JsonConvert.DeserializeObject<List<PlayList>>(response.Content.ReadAsString());
        }

        public IEnumerable<Label> GetLabels()
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/labels");

            HttpResponseMessage response = SendRequest(request);
            Labels labels = JsonConvert.DeserializeObject<Labels>(response.Content.ReadAsString());

            //The brightcove API only returns leaf nodes so we have to generate the rest of the directory tree...
            HashSet<string> uniquePaths = new HashSet<string>();

            foreach (string path in labels.Paths)
            {
                List<string> subPaths = path.Split('/').ToList();

                for (int i = subPaths.Count - 1; i > 1; i--)
                {
                    subPaths.RemoveAt(i);
                    uniquePaths.Add(string.Join("/", subPaths));
                }
            }

            return uniquePaths.Select(p => new Label(p)).ToList();
        }

        public IEnumerable<Folder> GetFolders()
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/folders");

            HttpResponseMessage response = SendRequest(request);

            return JsonConvert.DeserializeObject<List<Folder>>(response.Content.ReadAsString());
        }

        public IEnumerable<Video> GetVideos(int offset = 0, int limit = 20, string sort = "", string query = "")
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos?offset={offset}&limit={limit}&sort={sort}&query={query}");

            HttpResponseMessage response = SendRequest(request);

            return JsonConvert.DeserializeObject<List<Video>>(response.Content.ReadAsString());
        }

        public IEnumerable<VideoVariant> GetVideoVariants(string videoId)
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos/{videoId}/variants");

            HttpResponseMessage response = SendRequest(request);

            var results = JsonConvert.DeserializeObject<List<VideoVariant>>(response.Content.ReadAsString());
            results.ForEach(r => r.Id = videoId);

            return results;
        }

        public PlayerList GetPlayers()
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{playersBaseUrl}/{accountId}/players");

            HttpResponseMessage response = SendRequest(request);

            PlayerList players = JsonConvert.DeserializeObject<PlayerList>(response.Content.ReadAsString());

            return players;
        }

        public ExperienceList GetExperiences(string query = "", string sort = "")
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{experienceBaseUrl}/{accountId}/experiences?query={query}&sort={sort}");

            HttpResponseMessage response = SendRequest(request);

            return JsonConvert.DeserializeObject<ExperienceList>(response.Content.ReadAsString());
        }

        public bool TryGetPlayer(string playerId, out Player player)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                player = null;
                return false;
            }

            if (playerId.Contains(","))
            {
                throw new ArgumentException("the video ID must not contain any commas", nameof(playerId));
            }

            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response;

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{playersBaseUrl}/{accountId}/players/{playerId}");

            try
            {
                response = SendRequest(request);
            }
            catch (HttpStatusException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    player = null;
                    return false;
                }

                throw ex;
            }

            player = JsonConvert.DeserializeObject<Player>(response.Content.ReadAsString());
            return true;
        }

        public bool TryGetFolder(string folderId, out Folder folder)
        {
            if (string.IsNullOrWhiteSpace(folderId))
            {
                folder = null;
                return false;
            }

            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response;

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/folders/{folderId}");

            try
            {
                response = SendRequest(request);
            }
            catch (HttpStatusException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    folder = null;
                    return false;
                }

                throw ex;
            }

            folder = JsonConvert.DeserializeObject<Folder>(response.Content.ReadAsString());
            return true;
        }

        public bool TryGetVideo(string videoId, out Video video)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                video = null;
                return false;
            }

            if (videoId.Contains(","))
            {
                throw new ArgumentException("the video ID must not contain any commas", nameof(videoId));
            }

            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response;

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos/{videoId}");

            try
            {
                response = SendRequest(request);
            }
            catch (HttpStatusException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    video = null;
                    return false;
                }

                throw ex;
            }

            video = JsonConvert.DeserializeObject<Video>(response.Content.ReadAsString());
            return true;
        }


        public bool TryGetVideoVariant(string videoId, string language, out VideoVariant videoVariant)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                videoVariant = null;
                return false;
            }

            if (videoId.Contains(","))
            {
                throw new ArgumentException("the video ID must not contain any commas", nameof(videoId));
            }

            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response;

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/videos/{videoId}/variants/{language}");

            try
            {
                response = SendRequest(request);
            }
            catch (HttpStatusException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    videoVariant = null;
                    return false;
                }

                throw ex;
            }

            videoVariant = JsonConvert.DeserializeObject<VideoVariant>(response.Content.ReadAsString());
            videoVariant.Id = videoId;

            return true;
        }

        public bool TryGetPlaylist(string playlistId, out PlayList playlist)
        {
            if (string.IsNullOrWhiteSpace(playlistId))
            {
                playlist = null;
                return false;
            }

            if (playlistId.Contains(","))
            {
                throw new ArgumentException("the playlist ID must not contain any commas", nameof(playlistId));
            }

            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response;

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/playlists/{playlistId}");

            try
            {
                response = SendRequest(request);
            }
            catch (HttpStatusException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    playlist = null;
                    return false;
                }

                throw ex;
            }

            playlist = JsonConvert.DeserializeObject<PlayList>(response.Content.ReadAsString());
            return true;
        }

        public int VideosCount(string query = "")
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/counts/videos?query={query}");

            HttpResponseMessage response = SendRequest(request);
            Count count = JsonConvert.DeserializeObject<Count>(response.Content.ReadAsString());

            return count.Value;
        }

        public int PlayListsCount()
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{cmsBaseUrl}/{accountId}/counts/playlists");

            HttpResponseMessage response = SendRequest(request);
            Count count = JsonConvert.DeserializeObject<Count>(response.Content.ReadAsString());

            return count.Value;
        }

        private HttpResponseMessage SendRequest(HttpRequestMessage request)
        {
            try
            {
                request.Headers.Authorization = authenticationService.CreateAuthenticationHeader();

                HttpResponseMessage response = client.Send(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpStatusException(request, response);
                }

                return response;
            }
            catch(Exception ex)
            {
                if(retryAttempt < retryMax)
                {
                    retryAttempt++;
                    return SendRequest(request);
                }

                retryAttempt = 0;
                throw ex;
            }
        }
    }
}

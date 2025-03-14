using DataAccessLayer;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public interface IPostsService
    {
        Task<List<PostDto>> GetAllPostsAsync();
        Task<PostDto?> GetPostByIdAsync(int postId);
        Task<int> AddNewPostAsync(PostDto post);
        Task<bool> UpdatePostAsync(PostDto post);
        Task<bool> DeletePostAsync(int postId);
        Task<bool> IsPostExistsByIdAsync(int postId);
    }

    public class PostsService : IPostsService
    {
        private readonly IPostsRepository _postsRepository;

        public PostsService(IPostsRepository repository)
        {
            _postsRepository = repository;
        }

        public async Task<List<PostDto>> GetAllPostsAsync() =>
            await _postsRepository.GetAllPostsAsync();

        public async Task<PostDto?> GetPostByIdAsync(int postId) =>
            await _postsRepository.GetPostByIdAsync(postId);

        public async Task<int> AddNewPostAsync(PostDto post) =>
            await _postsRepository.AddNewPostAsync(post);

        public async Task<bool> UpdatePostAsync(PostDto post) =>
            await _postsRepository.UpdatePostAsync(post);

        public async Task<bool> DeletePostAsync(int postId) =>
            await _postsRepository.DeletePostAsync(postId);

        public async Task<bool> IsPostExistsByIdAsync(int postId) =>
            await _postsRepository.IsPostExistsByIdAsync(postId);
    }
}
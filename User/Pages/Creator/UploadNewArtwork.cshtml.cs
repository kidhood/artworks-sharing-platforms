using ArtHubBO.Constants;
using ArtHubBO.Entities;
using ArtHubBO.Enum;
using ArtHubBO.Models;
using ArtHubBO.DTO;
using ArtHubService.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using User.Pages.Filter;

namespace User.Pages.Creator;

public class UploadNewArtworkModel : PageModel
{
    private readonly IConfiguration configuration;
    private readonly IStorageService storageService;
    private readonly ICategoryService categoryService;
    private readonly IPostService postService;

    [BindProperty]    
    public IFormFile FileUpload { get; set; }

    [BindProperty]
    public List<Category> Categories { get; set; }

    [BindProperty]
    public PostScope PostScopePublic { get; } = PostScope.Public;
    [BindProperty]
    public PostScope PostScopeSubscriber { get; } = PostScope.Subscriber;
    [BindProperty]
    public PostScope PostScopePrivate { get; } = PostScope.Private;

    [BindProperty]
    public PostUpdateDto Post { get; set; }

    public UploadNewArtworkModel(IConfiguration configuration, IStorageService storageService, ICategoryService categoryService, IPostService postService)
    {
        this.configuration = configuration;
        this.storageService = storageService;
        this.categoryService = categoryService;
        this.postService = postService;
    }

    public async Task<IActionResult> OnGet()
    {
        Categories = categoryService.GetCategories().ToList();
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (FileUpload == null)
        {
            ViewData["ErrorMessageFileUpload"] = "Image upload is required!";
            return Page();
        }
        var selectedCategoryIds = Request.Form["SelectedCategories"].Select(categoryId =>
        {
            if (int.TryParse(categoryId, out int parsedCategoryId))
            {
                return parsedCategoryId;
            }
            else
            {
                return -1;
            }
        })
        .Where(categoryId => categoryId != -1)
        .ToArray();

        if (selectedCategoryIds.Count() <= 0)
        {
            ViewData["ErrorMessageCategories"] = "Please choose at least one category!";
            return Page();
        }

        var postScope = Request.Form["PostScope"];
        if (Enum.TryParse(postScope, out PostScope parsedScope))
        {
            Post.Scope = (int)parsedScope;
        } else
        {
            ViewData["ErrorMessagePostScope"] = "Please choose post scope!";
            return Page();
        }

        var accountEmail = HttpContext.Session.GetString("ACCOUNT_EMAIL");
        if (accountEmail != null)
        {
            Post.ArtistEmail = accountEmail;
        }
        await using var memoryStream = new MemoryStream();
        await FileUpload.CopyToAsync(memoryStream);
        var fileExt = Path.GetExtension(FileUpload.FileName);
        var objName = $"{S3Constants.FolderS3}{Guid.NewGuid()}{fileExt}";
        var s3Obj = new S3ObjectModel()
        {
            BucketName = configuration[S3Constants.BucketName] ?? "",
            InputStream = memoryStream,
            Name = objName
        };

        var credential = new AwsCredentials()
        {
            AwsKey = configuration[S3Constants.AccessKey] ?? "",
            AwsSecret = configuration[S3Constants.SecretKey] ?? ""
        };
        var responseUploadImage = await storageService.UploadFileAsync(s3Obj, credential);         

        // Post category
        List<PostCategory> postCategories = new List<PostCategory>();
        foreach(var id in selectedCategoryIds) {
            postCategories.Add(new PostCategory
            {
                CategoryId = id
            });
        }
        //Image of post
        Image image = new Image()
        {
            Type = fileExt[1..],
            ImageUrl = responseUploadImage.LinkSource,            
            DeleteFlag = false
        };
        Post.Images = new List<Image>
        {
            image
        };
        Post.PostCategories = new List<PostCategory>();
        postCategories.ForEach(pc => Post.PostCategories.Add(pc));                        
        await postService.CreateNewPost(ConvertPostUpdateDtoToPost(Post));
        return RedirectToPage(URIConstant.ArtworkList);
    }

    private Post ConvertPostUpdateDtoToPost(PostUpdateDto postUpdateDto)
    {
        return new Post()
        {
            Title = postUpdateDto.Title,
            Description = postUpdateDto.Description,
            Status = (int)PostStatus.Pending,
            Scope = postUpdateDto.Scope,
            TotalReact = 0,
            TotalBookmark = 0,
            TotalView = 0,
            ArtistEmail = postUpdateDto.ArtistEmail,
            Images = postUpdateDto.Images,
            PostCategories = postUpdateDto.PostCategories
        };
    }
}

﻿using System.Diagnostics;
using ArtHubBO.DTO;
using ArtHubBO.Entities;
using ArtHubBO.Enum;
using ArtHubBO.Payload;
using ArtHubService.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace User.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IPostService postService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IPostService postService, ILogger<IndexModel> logger) : base()
        {
            this.postService = postService;
            _logger = logger;
        }

        public IList<Post> Posts { get; set; } = default!;
        public SearchPayload<PostSearchConditionDto> SearchPayload = default!;


        public async Task OnGetAsync(int pageIndex = 1, int pageSize = 12, string? search = "", int? orderBy = null, int[]? category = null)
        {
            PostSearchConditionDto condition = new PostSearchConditionDto()
            {
                PostScope = PostScope.Public,
                PostStatus = PostStatus.Approval,
                SortType = orderBy == (int)SortType.FAVOURITE ? SortType.FAVOURITE : SortType.RECENT,
                SortDirection = SortDirection.DESC,
                Title = search,
                CategoryId = category
            };

            SearchPayload = new SearchPayload<PostSearchConditionDto>()
            {
                PageInfo = new PageInfo()
                {
                    PageNum = pageIndex,
                    PageSize = pageSize,
                },
                SearchCondition = condition
            };

            Posts = await postService.GetAllPostBySearchConditionAsync(SearchPayload);

            _logger.LogInformation("Log ne cu version: 1.1");
        }
    }
}

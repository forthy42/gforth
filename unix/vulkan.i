// this file is in the public domain
%module gl
%insert("include")
%{
#include <vulkan/vulkan.h>
%}

%apply unsigned long long { uint64_t };
%apply unsigned long { size_t };
%apply int { int32_t };
%apply unsigned int { uint32_t };
%apply SWIGTYPE * { VkBuffer,VkBufferView,VkCommandPool,VkDebugReportCallbackEXT,VkDescriptorPool,VkDescriptorSetLayout,VkDeviceMemory,VkDisplayKHR,VkDisplayModeKHR,VkEvent,VkFence,VkFramebuffer,VkImage,VkImageView,VkPipeline,VkPipelineCache,VkPipelineLayout,VkQueryPool,VkRenderPass,VkSampler,VkSemaphore,VkShaderModule,VkSurfaceKHR,VkSwapchainKHR,VkIndirectCommandsLayoutNVX,VkObjectTableNVX };
#define VKAPI_PTR
#define VKAPI_ATTR
#define VKAPI_CALL

%include <vulkan/vk_platform.h>
%include <vulkan/vulkan.h>

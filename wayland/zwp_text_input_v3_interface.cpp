#include <wayland-client-protocol.hpp>
#include <wayland-client-protocol-unstable.hpp>

using namespace wayland;
using namespace detail;

extern "C" void * get_zwp_text_input_v3_interface(void)
{
  return (void*)&wayland::detail::zwp_text_input_v3_interface;
}

extern "C" void * get_zwp_text_input_manager_v3_interface(void)
{
  return (void*)&wayland::detail::zwp_text_input_manager_v3_interface;
}

#define ZWP_TEXT_INPUT_MANAGER_V3_GET_TEXT_INPUT 1

extern "C" struct zwp_text_input_v3 *
get_zwp_text_input_manager_v3_get_text_input(struct zwp_text_input_manager_v3 *zwp_text_input_manager_v3, struct wl_seat *seat)
{
	struct wl_proxy *id;

	id = wl_proxy_marshal_flags((struct wl_proxy *) zwp_text_input_manager_v3,
			 ZWP_TEXT_INPUT_MANAGER_V3_GET_TEXT_INPUT, &wayland::detail::zwp_text_input_v3_interface, wl_proxy_get_version((struct wl_proxy *) zwp_text_input_manager_v3), 0, NULL, seat);

	return (struct zwp_text_input_v3 *) id;
}

#include <signalrclient/hub_connection.h>

int main()
{
	signalr::hub_connection connection{ U("http://localhost:14807") };
	auto proxy = connection.create_hub_proxy(U("GameHub"));
	proxy.on(U("broadcastMessage"), [](const web::json::value& m) {
		ucout << std::endl << m.at(0).as_string() << U(" wrote:") << m.at(1).as_string() << std::endl << U("Enter your message: ");
	});

	connection.start()
		.then([&proxy]() {
			web::json::value args{};
			args[0] = web::json::value::string(L"key");
			args[1] = web::json::value(8);

			proxy.invoke<void>(U("updatePlayer"), args)
				.then([](pplx::task<void> invokeTask)
			{
				try {
					invokeTask.get();
				} catch (const std::exception &e) {
					ucout << U("Error while sending data: ") << e.what();
				}
			});
		}).then([&connection]() {
		// fine to capture by reference - we are blocking so it is guaranteed to be valid
		return connection.stop();
	}).then([](pplx::task<void> stop_task) {
		try {
			stop_task.get();
			ucout << U("connection stopped successfully") << std::endl;
		} catch (const std::exception &e) {
			ucout << U("exception when starting or stopping connection: ") << e.what() << std::endl;
		}
	}).get();

	return 0;
}
We are given a GitHub issue: #48: Add healthcheck for Sockets and "live" status bubble on Socket page.

We are to edit ONLY the file: src\HomelabDocs.Server\HomelabDocs.Business\Sockets\SocketService.cs

We must make the SMALLEST correct change and reuse existing style/imports.

The issue has two parts:
1. Add healthcheck for Sockets.
2. Add a "live" status bubble on the Socket page.

However, note that we are only allowed to edit the given file: SocketService.cs.

Looking at the provided code snippet, we see that the SocketService class currently has:
- ListAsync
- GetAsync
- CreateAsync
- UpdateAsync (incomplete in the snippet, but we are only allowed to edit the given file and we see the UpdateAsync method is cut off)

But note: the issue says to add a healthcheck for Sockets and a live status bubble.

Since we are only allowed to change SocketService.cs, we must consider what changes in this file would support these features.

Let's break down:

Part 1: Healthcheck for Sockets.
Typically, a healthcheck might involve checking if the socket service is able to communicate with the sockets (i.e., the addresses) or if the repository is working. However, note that the healthcheck might be implemented in a separate healthcheck service. But the issue says to add it for Sockets.

Alternatively, we might need to add a method in the SocketService that performs a health check (e.g., pinging the socket address) and returns a status.

Part 2: "live" status bubble on the Socket page.
This likely means that on the UI page that lists sockets, we want to show a bubble (like a colored dot) indicating whether the socket is currently reachable (live) or not.

To support the live status, we would need to have a way to check the liveness of a socket (e.g., by trying to connect to the address) and then include that information in the SocketResponse.

Looking at the existing code, we have a SocketResponse type (from HomelabDocs.Shared.Sockets). We don't have the definition of SocketResponse in the given file, but we can assume it is defined elsewhere.

We are allowed to reuse existing style and imports. We cannot add new files.

Therefore, we must consider:

Option 1: Modify the SocketResponse to include a live status property. But we cannot change SocketResponse because it's in a shared project and we are only allowed to edit SocketService.cs.

Option 2: Instead, we might add a new method to SocketService that returns the live status for a socket, and then the UI can call that method. However, the issue says to show the live status bubble on the Socket page (which likely lists sockets). So we would need to include the live status in the list of sockets.

But note: we are only allowed to change SocketService.cs. We cannot change the SocketResponse if it's defined elsewhere.

Wait: let's look at the imports in the given file:
using HomelabDocs.Domain.Sockets;
using HomelabDocs.Shared.Sockets;

So SocketResponse is in HomelabDocs.Shared.Sockets. We cannot change that file because we are only allowed to edit SocketService.cs.

Therefore, we must find another way.

Alternatively, we might extend the SocketResponse by creating a new type? But we cannot create new files.

Another idea: perhaps the healthcheck and live status are not about the socket's network liveness but about the service's health? The issue says "healthcheck for Sockets", which might mean a healthcheck endpoint that checks the socket service (including the ability to list sockets, etc.). And the live status bubble might be a misnomer? Or it might be about the socket's last seen time?

But note: the issue says "live" status bubble, which implies real-time liveness.

Given the constraints, we must work within SocketService.cs.

Let me reexamine the provided code snippet: the UpdateAsync method is incomplete. We are only given the beginning of it. But we are allowed to edit the entire file? The instruction says: "Edit ONLY the file below", meaning we are to replace the entire file with a corrected version? But note: we must make the SMALLEST correct change.

However, the problem says: "Return the COMPLETE updated file in one fenced code block."

So we are to write the entire file as it should be after our changes.

But note: the provided snippet ends with:
    public async Task<SocketMutationResult> UpdateAsync(
        
We don't see the rest. We must assume that the rest of the file is as it was, but we are only allowed to change SocketService.cs and we are given the beginning of the UpdateAsync method.

However, to make the smallest change, we should only change what is necessary.

But wait: the issue requires two features. How can we implement both by only changing SocketService.cs?

Idea for healthcheck:
We can add a new method to SocketService: Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
This method would check the health of the socket service. For example, it might try to list the sockets (to check the repository) and return a healthy status if that works.

Idea for live status:
We cannot change SocketResponse, so we cannot add a live property there. Therefore, we must consider that the live status might be shown in a different way? Or perhaps we are misunderstanding.

Alternatively, the live status bubble might be shown on the socket detail page (not the list). The issue says "on Socket page", which could be the list or the detail.

But even on the detail page, we would need to get the live status for a specific socket. We could add a method to SocketService: Task<bool> IsLiveAsync(Guid socketId, CancellationToken cancellationToken = default) that pings the socket address.

Then, the UI could call that method for the socket when displaying the detail.

But the issue says to show the bubble on the Socket page (which might be the list). If it's the list, then we would need to get the live status for every socket in the list. That would be inefficient if we do it one by one, but we could do it in bulk? However, we don't have a method for bulk liveness check.

Given the constraints, we are to make the smallest change. We are only allowed to change SocketService.cs.

Let's assume that the healthcheck is a separate concern and the live status is for the detail view. Then we can:

1. Add a healthcheck method (for the service health) that checks the repository.
2. Add a method to check the liveness of a single socket (by trying to connect to its address).

But note: the issue says "healthcheck for Sockets" (plural) and then "live" status bubble (singular, per socket). So:

- The healthcheck for Sockets might be a service-level healthcheck (like: is the socket service working?).
- The live status bubble is per socket.

Therefore, we can do:

In SocketService.cs:

Add:
   public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
   {
        try
        {
            // Check if we can list at least one socket? Or just that the repository is responsive?
            await _socketRepository.ListAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
   }

And for the live status of a socket:
   public async Task<bool> IsLiveAsync(Guid id, CancellationToken cancellationToken = default)
   {
        var socket = await _socketRepository.GetByIdAsync(id, cancellationToken);
        if (socket is null)
        {
            return false;
        }

        // Try to connect to socket.Address? We don't have a TCP client here.
        // We would need to implement a ping or a port check.
        // But note: we are not allowed to change other files and we cannot add new dependencies arbitrarily?
        // However, we can use System.Net.Sockets.Socket? But that might be heavy and we don't want to block.

        // Alternatively, we might assume that the socket address is a URL and we can do an HTTP request? 
        // But the SocketEntity has an Address property. We don't know what it is.

        // Since we are making the smallest change and we don't have context, we might leave the implementation as a placeholder?
        // But the issue requires it to be correct.

        // However, note: we are only allowed to change SocketService.cs. We cannot change the SocketEntity to add a LastSeen or something.

        // Given the ambiguity, and since we are to make the smallest change, we might implement a simple TCP connection attempt?
        // But we don't know the port. The Address might be a hostname or IP without port.

        // Looking at the CreateAsync method: we have address as a string. It is trimmed and stored.

        // Without more context, we cannot implement a real liveness check. Therefore, we must assume that the live status is not about network liveness but about something else?

        // Alternatively, the live status might be derived from the socket's last activity? But we don't have that in SocketEntity.

        // Let me look at the SocketEntity: we have CreatedAt. We don't have UpdatedAt or LastSeen.

        // Therefore, we cannot compute liveness from existing data.

        // This suggests that the live status bubble might be intended to be implemented by checking the socket's address via a network call.

        // We are going to have to do that. We'll use System.Net.Sockets.Socket to attempt a TCP connection to the address on a default port? 
        // But what port? The issue doesn't specify.

        // Alternatively, the Address might include the port? For example, "example.com:8080".

        // We can try to parse the address: if it contains a colon, split and use the port; otherwise, use a default port (like 80 for HTTP? but we don't know the protocol).

        // This is getting complicated and might be beyond the scope of the smallest change.

        // Given the instructions, we must make the smallest change that resolves the issue. We are allowed to assume that the Address is in the format "host:port" or just "host" (and then we use a default port?).

        // However, note: we are not given any context about the address format.

        // Since we are to make the smallest change, we might leave the liveness check as a stub that always returns true? But that would not be correct.

        // Alternatively, we might note that the issue might be interpreted differently.

        // Let me reread the issue: "Add healthcheck for Sockets and 'live' status bubble on Socket page."

        // Another interpretation: 
        //   - The healthcheck for Sockets: we add a healthcheck endpoint that checks the socket service (which we can do by checking the repository).
        //   - The live status bubble: we add a property to the SocketResponse that indicates if the socket is currently being updated or something? But we cannot change SocketResponse.

        // Given the constraints, I think we have to change the SocketResponse? But we cannot because it's in a shared project and we are only allowed to edit SocketService.cs.

        // Wait: what if we create a new class that inherits from SocketResponse? But we cannot create new files.

        // Alternatively, we might use a different approach: the UI might call a separate endpoint for the live status. Then we don't need to change SocketResponse.

        // We can add a method to SocketService: Task<SocketLiveStatus> GetLiveStatusAsync(Guid id, ...) and then the UI can call that for the detail page.

        // But the issue says "on Socket page", which might be the list page. If it's the list page, then we would need to get the live status for every socket in the list. We could add a method that returns a list of sockets with live status? But we cannot change SocketResponse.

        // We are stuck.

        // Let me look again at the provided code snippet: we are only given the beginning of the UpdateAsync method. The rest of the file might contain more methods? We don't know.

        // We are to return the COMPLETE updated file. We must assume that the file currently ends with the UpdateAsync method (incomplete) and we have to complete it and add our changes.

        // But note: the issue says to make the SMALLEST correct change. We are allowed to complete the UpdateAsync method as it was? Because it was cut off.

        // However, the provided snippet ends with:
        //    public async Task<SocketMutationResult> UpdateAsync(
        //        
        // We don't see the parameters. We must assume that the UpdateAsync method was intended to have parameters (like id, name, address) and then a body.

        // Since we are only allowed to change SocketService.cs, and we are to make the smallest change, we should:
        //   1. Complete the UpdateAsync method as it was originally (if we knew what it was) OR leave it as is and complete it in a way that matches the pattern? But we don't have the original.

        // Given that we don't have the rest of the file, we must assume that the file we are given is the entire file? But it ends mid-method.

        // This is confusing.

        // Alternative approach: since we are to edit ONLY the file below and return the COMPLETE updated file, we must write the entire file as we believe it should be after our changes.

        // We are given the beginning of the file. We must preserve the existing methods and add our new methods.

        // Steps:
        //   - We will complete the UpdateAsync method in a way that matches the CreateAsync method (since we have CreateAsync as a model).
        //   - We will add two new methods: CheckHealthAsync

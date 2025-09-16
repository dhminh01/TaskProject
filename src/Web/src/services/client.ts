import { ApolloClient, HttpLink, InMemoryCache } from "@apollo/client";

// Create Apollo Client with cache configuration
const client = new ApolloClient({
  link: new HttpLink({ uri: "http://localhost:5000/tasks" }),
  cache: new InMemoryCache({
    typePolicies: {
      Query: {
        fields: {
          tasks: {
            // Merge function to properly handle task lists
            merge(_existing = [], incoming: any[]) {
              return [...incoming];
            },
          },
        },
      },
      Task: {
        // Unique identifier for Task type
        keyFields: ["id"],
        fields: {
          title: {
            // Read function to check for duplicate titles
            read(title) {
              return title;
            },
          },
        },
      },
    },
  }),
  defaultOptions: {
    watchQuery: {
      fetchPolicy: "cache-and-network", // Ensures we always have fresh data while showing cached data immediately
    },
  },
});

// Import the GET_TASKS query
import { GET_TASKS } from "../graphql/queries";

// Pre-fetch tasks when the client is created
client
  .query({
    query: GET_TASKS,
  })
  .catch((error) => {
    console.error("Error prefetching tasks:", error);
  });

export default client;

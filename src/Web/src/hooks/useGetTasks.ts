import { useQuery } from "@apollo/client/react";
import { GET_TASKS } from "../graphql/queries";
import type { GetTasksData } from "../types";

export function useGetTasks() {
  return useQuery<GetTasksData>(GET_TASKS);
}
